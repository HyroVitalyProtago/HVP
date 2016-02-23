using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

/// <summary>
/// Event conductor is useful to abstract event's links between objects.
/// Because an object generally don't need to know who activate the event,
/// this class play the role of middleman to attach and detach events.
/// 
/// How to use it?
/// 
/// Talkers:
/// [static] event Action OnEvent;
/// EventConductor.(Offer|Denial)((this|GetType()), "OnEvent");
/// 
/// Listeners:
/// EventConductor.(On|Off)((this|GetType()), "OnDeath" [, "OnDeathEvent"]);
/// </summary>
/// <author email="hyrovitalyprotago@gmail.com">Hyro Vitaly Protago</author>
public static class EventConductor {

	public class EventNotFoundException : Exception { }
	public class EventNotRegisteredException : Exception { }
	// public class EventAlreadyOfferedException : Exception { }
	public class CallbackNotFoundException : Exception { }
	public class CallbackBadTypeException : Exception { }
	// public class CallbackAlreadyConnectedException : Exception { }
	public class CallbackNotRegisteredException : Exception { }
	public class EventNotMatchCallbackException : Exception { }

	public static readonly string EventPrefixAdd = "add_";
	public static readonly int EventAddID = 0;
	public static readonly string EventPrefixRemove = "remove_";
	public static readonly int EventRemoveID = 1;

	public static readonly BindingFlags StaticPublic = BindingFlags.Static | BindingFlags.Public;
	public static readonly BindingFlags InstancePublic = BindingFlags.Instance | BindingFlags.Public;

	/// <summary>
	/// The static talkers correspond to classes who launch static events.
	/// </summary>
	static readonly Dictionary<Type, Dictionary<string, MethodInfo[]>> StaticTalkers =
		new Dictionary<Type, Dictionary<string, MethodInfo[]>>();

	/// <summary>
	/// The dynamic talkers correspond to classes who launch events which refer to them.
	/// </summary>
	static readonly Dictionary<object, Dictionary<string, MethodInfo[]>> DynamicTalkers =
		new Dictionary<object, Dictionary<string, MethodInfo[]>>();

	/// <summary>
	/// The listeners correspond to classes who attend some events for fire callbacks.
	/// There can be two kinds of listeners : Type (for static method) and Object (for instance method)
	/// </summary>
	static readonly Dictionary<string, Dictionary<object, Delegate>> Listeners =
		new Dictionary<string, Dictionary<object, Delegate>>();

	#region Talkers

	static void AbstractOffer<T>(
		T talker,
		string eventName,
		BindingFlags flags,
		IDictionary<T, Dictionary<string, MethodInfo[]>> talkers
	) {
		if (talker == null || eventName == null) {
			throw new ArgumentNullException();
		}

		Type typ = talker is Type ? talker as Type : talker.GetType();

		MethodInfo eventAdd = typ.GetMethod(EventPrefixAdd + eventName, flags);
		MethodInfo eventRemove = typ.GetMethod(EventPrefixRemove + eventName, flags);
		if (eventAdd == null || eventRemove == null) {
			throw new EventNotFoundException();
		}

		// Connect all listeners
		if (Listeners.ContainsKey(eventName)) {
			object invokedTalker = talker is Type ? null : talker as object;
			foreach (var pair in Listeners[eventName]) {
				try {
					eventAdd.Invoke(invokedTalker, new object[] { pair.Value });
				} catch (ArgumentException) {
					throw new EventNotMatchCallbackException();
				}
			}
		}

		// Add in static talkers
		if (!talkers.ContainsKey(talker)) {
			talkers[talker] = new Dictionary<string, MethodInfo[]>();
		}
		talkers[talker].Add(eventName, new[] { eventAdd, eventRemove });
	}

	static void AbstractDenial<T>(
		T talker,
		string eventName,
		IDictionary<T, Dictionary<string, MethodInfo[]>> talkers
	) {
		if (talker == null || eventName == null) {
			throw new ArgumentNullException();
		}

		if (!talkers.ContainsKey(talker) || !talkers[talker].ContainsKey(eventName)) {
			throw new EventNotRegisteredException();
		}

		// Disconnect all listeners
		if (Listeners.ContainsKey(eventName)) {
			object invokedTalker = talker is Type ? null : talker as object;
			foreach (var pair in Listeners[eventName]) {
				talkers[talker][eventName][EventRemoveID].Invoke(invokedTalker, new object[] { pair.Value });
			}
		}

		talkers[talker].Remove(eventName);
	}

	public static void Offer(Type typ, string eventName) {
		AbstractOffer(typ, eventName, StaticPublic, StaticTalkers);
	}

	public static void Denial(Type typ, string eventName) {
		AbstractDenial(typ, eventName, StaticTalkers);
	}

	public static void Offer(object self, string eventName) {
		AbstractOffer(self, eventName, InstancePublic, DynamicTalkers);
	}

	public static void Denial(object self, string eventName) {
		AbstractDenial(self, eventName, DynamicTalkers);
	}

	#endregion

	#region Listeners

	static void AbstractOn(object self, string eventName, string callbackName, BindingFlags flags) {
		if (self == null || eventName == null || callbackName == null) {
			throw new ArgumentNullException();
		}

		Type typ = self is Type ? self as Type : self.GetType();

		MethodInfo method = typ.GetMethod(callbackName, flags);
		if (method == null) {
			throw new CallbackNotFoundException();
		}

		Delegate callback;
		try {
			if (self is Type) {
				callback = Delegate.CreateDelegate(DelegateType(method), typ, callbackName);
			} else {
				callback = Delegate.CreateDelegate(DelegateType(method), self, callbackName);
			}
		} catch (ArgumentException) {
			throw new CallbackBadTypeException();
		} // MethodAccessException

		// Connect all talkers
		foreach (var pair in StaticTalkers.Where(pair => pair.Value.ContainsKey(eventName))) {
			pair.Value[eventName][EventAddID].Invoke(null, new object[] { callback });
		}
		foreach (var pair in DynamicTalkers.Where(pair => pair.Value.ContainsKey(eventName))) {
			pair.Value[eventName][EventAddID].Invoke(pair.Key, new object[] { callback });
		}

		// Add in listeners
		if (!Listeners.ContainsKey(eventName)) {
			Listeners[eventName] = new Dictionary<object, Delegate>();
		}
		Listeners[eventName].Add(self, callback);
	}

	static void AbstractOff(object obj, string eventName) {
		if (obj == null || eventName == null) {
			throw new ArgumentNullException();
		}

		if (!Listeners.ContainsKey(eventName) || !Listeners[eventName].ContainsKey(obj)) {
			throw new CallbackNotRegisteredException();
		}

		// Disconnect all talkers
		foreach (var pair in StaticTalkers.Where(pair => pair.Value.ContainsKey(eventName))) {
			pair.Value[eventName][EventRemoveID].Invoke(null, new object[] { Listeners[eventName][obj] });
		}
		foreach (var pair in DynamicTalkers.Where(pair => pair.Value.ContainsKey(eventName))) {
			pair.Value[eventName][EventRemoveID].Invoke(pair.Key, new object[] { Listeners[eventName][obj] });
		}

		Listeners[eventName].Remove(obj);
	}

	public static void On(Type typ, string eventName, string callbackName = null) {
		AbstractOn(typ, eventName, callbackName == null ? eventName : callbackName, StaticPublic);
	}

	public static void Off(Type typ, string eventName) {
		AbstractOff(typ, eventName);
	}

	public static void On(object self, string eventName, string callbackName = null) {
		AbstractOn(self, eventName, callbackName == null ? eventName : callbackName, InstancePublic);
	}

	public static void Off(object self, string eventName) {
		AbstractOff(self, eventName);
	}

	#endregion

	public static void Send(string eventName, params object[] parameters) {
		if (!Listeners.ContainsKey(eventName)) {
			UnityEngine.Debug.LogWarning("An event wasn't listened: "+eventName);
			return;
		}
		foreach (var pair in Listeners[eventName]) {
			pair.Value.DynamicInvoke(parameters);
		}
	}

	public static Type DelegateType(MethodInfo method) {
		List<Type> args = new List<Type>(method.GetParameters().Select(p => p.ParameterType));
		if (method.ReturnType == typeof(void)) {
			return Expression.GetActionType(args.ToArray());
		}
		args.Add(method.ReturnType);
		return Expression.GetFuncType(args.ToArray());
	}

    static readonly string[] NoEvent = { "No Event" };
    public static string[] GetEventsOf(Component component) {
        if (component == null) return NoEvent;
        return component.GetType()
                        .GetMembers(BindingFlags.Instance | BindingFlags.Public)
                        .Where(mi => mi.MemberType == MemberTypes.Event)
                        .Select(mi => mi.Name)
                        .ToArray()
                        .Prepend("No Event"); // TODO Prepend "No Event"
    }

    static readonly string[] NoCallback = { "No Callback" };
    public static string[] GetCallbacksOf(Component component) {
        if (component == null) return NoCallback;
        return component.GetType()
                        .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                        .Where(m => !m.IsSpecialName)
                        .Select(mi => mi.Name)
                        .ToArray()
                        .Prepend("No Callback");
    }
}