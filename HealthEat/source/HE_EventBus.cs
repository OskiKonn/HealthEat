using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HealthEat.Exceptions;

namespace HealthEat
{
    internal class HE_EventBus
    {
        private HE_EventBus()
        {
            sm_Instance = this;

            #if HE_DEBUG
            Console.WriteLine($"[HE_EventBus]: EventBus created");
            #endif
        }

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

        public void SubscribeToEvent<TEvent>(Action<TEvent> subscriber)
            where TEvent : IHE_EventType
        {
            int id = HE_EventTypeCache<TEvent>.Id;

            if (!IsEventRegistered(id))
                throw new HE_InvalidArgumentValueException($"[HE_EventBus]: No event of type {typeof(TEvent).Name} to be subscribed");

            ((HE_Event<TEvent>)m_EventChannels[id]).Subscribe(subscriber);
        }

        public void SubscribeToEventForce<TEvent>(Action<TEvent> subscriber)
            where TEvent : IHE_EventType
        {
            int id = HE_EventTypeCache<TEvent>.Id;

            if (!IsEventRegistered(id))
                RegisterEvent<TEvent>();

            ((HE_Event<TEvent>)m_EventChannels[id]).Subscribe(subscriber);
        }

        public void UnsubscribeFromEvent<TEvent>(Action<TEvent> subscriber)
            where TEvent : IHE_EventType
        {
            int id = HE_EventTypeCache<TEvent>.Id;

            if (!IsEventRegistered(id))
                return;

            ((HE_Event<TEvent>)m_EventChannels[id]).Unsubscribe(subscriber);
        }

        public void BroadcastEvent<TEvent>(TEvent data)
            where TEvent : IHE_EventType
        {
            int id = HE_EventTypeCache<TEvent>.Id;

            if (!IsEventRegistered(id))
                return;

            ((HE_Event<TEvent>)m_EventChannels[id]).Broadcast(data);
        }

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

    internal interface IHE_EventType { }

    internal abstract class HE_EventBase
    {
        public abstract void Clear();
    }

    internal class HE_Event<TEvent> : HE_EventBase
        where TEvent : IHE_EventType
    {

        public void Subscribe(Action<TEvent> subscriber)
        {
            m_Subscribers += subscriber;
        }

        public void Unsubscribe(Action<TEvent> subscriber)
        {
            m_Subscribers -= subscriber;
        }

        public void Broadcast(TEvent ev)
        {
            m_Subscribers?.Invoke(ev);
        }

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
