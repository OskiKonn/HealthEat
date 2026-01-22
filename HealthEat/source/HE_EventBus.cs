using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HealthEat.Exceptions;

namespace HealthEat
{
    /// <summary>
    /// Central event bus for the game. Manages event registration, subscription, and broadcasting.
    /// Implements a singleton pattern.
    /// </summary>
    internal class HE_EventBus
    {
        /// <summary>
        /// Private constructor for singleton pattern.
        /// </summary>
        private HE_EventBus()
        {
            sm_Instance = this;

            #if HE_DEBUG
            Console.WriteLine($"[HE_EventBus]: EventBus created");
            #endif
        }

        /// <summary>
        /// Registers an event type with the event bus.
        /// </summary>
        /// <typeparam name="TEvent">The event type to register. Must implement IHE_EventType.</typeparam>
        public void RegisterEvent<TEvent>()
            where TEvent : IHE_EventType
        {
            int id = HE_EventTypeCache<TEvent>.Id;

            if (IsEventRegistered(id))
            {
                #if HE_DEBUG
                Console.WriteLine($"[HE_EventBus]: Event of type {typeof(TEvent).Name} is already registered to EventBus");
                #endif

                return;
            }

            AssertChannelsCapacity(id);

            m_EventChannels[id] = new HE_Event<TEvent>();
        }

        /// <summary>
        /// Unregisters an event type from the event bus.
        /// </summary>
        /// <typeparam name="TEvent">The event type to unregister.</typeparam>
        public void UnregisterEvent<TEvent>()
            where TEvent : IHE_EventType
        {
            int id = HE_EventTypeCache<TEvent>.Id;

            if (id >= m_EventChannels.Count)
            {
                #if HE_DEBUG
                Console.WriteLine($"[HE_EventBus]: No event of type {typeof(TEvent).Name} to be unregistered from EventBus");
                #endif

                return;
            }

            m_EventChannels[id]?.Clear();
            m_EventChannels[id] = null;
        }

        /// <summary>
        /// Subscribes a handler to an event type.
        /// </summary>
        /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
        /// <param name="subscriber">The action to invoke when the event is broadcast.</param>
        /// <exception cref="HE_InvalidArgumentValueException">Thrown when the event type is not registered.</exception>
        public void SubscribeToEvent<TEvent>(Action<TEvent> subscriber)
            where TEvent : IHE_EventType
        {
            int id = HE_EventTypeCache<TEvent>.Id;

            if (!IsEventRegistered(id))
                throw new HE_InvalidArgumentValueException($"[HE_EventBus]: No event of type {typeof(TEvent).Name} to be subscribed");

            ((HE_Event<TEvent>)m_EventChannels[id]).Subscribe(subscriber);
        }

        /// <summary>
        /// Subscribes a handler to an event type, registering the event if it doesn't exist.
        /// </summary>
        /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
        /// <param name="subscriber">The action to invoke when the event is broadcast.</param>
        public void SubscribeToEventForce<TEvent>(Action<TEvent> subscriber)
            where TEvent : IHE_EventType
        {
            int id = HE_EventTypeCache<TEvent>.Id;

            if (!IsEventRegistered(id))
                RegisterEvent<TEvent>();

            ((HE_Event<TEvent>)m_EventChannels[id]).Subscribe(subscriber);
        }

        /// <summary>
        /// Unsubscribes a handler from an event type.
        /// </summary>
        /// <typeparam name="TEvent">The event type to unsubscribe from.</typeparam>
        /// <param name="subscriber">The action to remove from the subscription list.</param>
        public void UnsubscribeFromEvent<TEvent>(Action<TEvent> subscriber)
            where TEvent : IHE_EventType
        {
            int id = HE_EventTypeCache<TEvent>.Id;

            if (!IsEventRegistered(id))
                return;

            ((HE_Event<TEvent>)m_EventChannels[id]).Unsubscribe(subscriber);
        }

        /// <summary>
        /// Broadcasts an event to all subscribed handlers.
        /// </summary>
        /// <typeparam name="TEvent">The event type to broadcast.</typeparam>
        /// <param name="data">The event data to broadcast.</param>
        public void BroadcastEvent<TEvent>(TEvent data)
            where TEvent : IHE_EventType
        {
            int id = HE_EventTypeCache<TEvent>.Id;

            if (!IsEventRegistered(id))
                return;

            ((HE_Event<TEvent>)m_EventChannels[id]).Broadcast(data);
        }

        /// <summary>
        /// Clears all event channels and subscriptions.
        /// </summary>
        public void ClearAll()
        {
            foreach (HE_EventBase? ev in m_EventChannels)
            {
                ev?.Clear();
            }
        }

        private void AssertChannelsCapacity(int eventId)
        {
            while (m_EventChannels.Count <= eventId)
            {
                m_EventChannels.Add(null);
            }
        }

        private bool IsEventRegistered(int id)
        {
            return (id < m_EventChannels.Count && m_EventChannels[id] != null);
        }

        /// <summary>
        /// Gets the singleton instance of the event bus.
        /// </summary>
        public static HE_EventBus Get
        {
            get
            {
                if (sm_Instance == null)
                {
                    sm_Instance = new HE_EventBus();
                }

                return sm_Instance;
            }
        }

        private static HE_EventBus sm_Instance = null!;
        private List<HE_EventBase?> m_EventChannels = new List<HE_EventBase?>(10);
    }

    /// <summary>
    /// Marker interface for all event types in the event bus system.
    /// </summary>
    internal interface IHE_EventType { }

    /// <summary>
    /// Base class for event channels in the event bus.
    /// </summary>
    internal abstract class HE_EventBase
    {
        /// <summary>
        /// Clears all subscribers from the event channel.
        /// </summary>
        public abstract void Clear();
    }

    /// <summary>
    /// Generic event channel for a specific event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type this channel handles.</typeparam>
    internal class HE_Event<TEvent> : HE_EventBase
        where TEvent : IHE_EventType
    {

        /// <summary>
        /// Subscribes a handler to this event channel.
        /// </summary>
        /// <param name="subscriber">The action to invoke when events are broadcast.</param>
        public void Subscribe(Action<TEvent> subscriber)
        {
            m_Subscribers += subscriber;
        }

        /// <summary>
        /// Unsubscribes a handler from this event channel.
        /// </summary>
        /// <param name="subscriber">The action to remove from subscriptions.</param>
        public void Unsubscribe(Action<TEvent> subscriber)
        {
            m_Subscribers -= subscriber;
        }

        /// <summary>
        /// Broadcasts an event to all subscribed handlers.
        /// </summary>
        /// <param name="ev">The event data to broadcast.</param>
        public void Broadcast(TEvent ev)
        {
            m_Subscribers?.Invoke(ev);
        }

        /// <summary>
        /// Clears all subscribers from this event channel.
        /// </summary>
        public override void Clear()
        {
            m_Subscribers = null;
        }

        private Action<TEvent>? m_Subscribers;
    }

    internal static class HE_EventIdCreator
    {

        public static int GetID()
        {
            return m_NextId++;
        }

        private static int m_NextId = 0;
    }

    internal static class HE_EventTypeCache<T>
    {
        public static readonly int Id = HE_EventIdCreator.GetID();
    }
}
