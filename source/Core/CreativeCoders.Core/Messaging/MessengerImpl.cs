﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CreativeCoders.Core.Collections;
using CreativeCoders.Core.Threading;
using CreativeCoders.Core.Weak;

namespace CreativeCoders.Core.Messaging;

internal class MessengerImpl : IMessenger
{
    private readonly ConcurrentDictionary<Type, IList<IMessengerRegistration>> _registrationsForMessages =
        new ConcurrentDictionary<Type, IList<IMessengerRegistration>>();

    internal void RemoveRegistration(IMessengerRegistration registration)
    {
        _registrationsForMessages.Values.ForEach(registrations => registrations.Remove(registration));
    }

    private IList<IMessengerRegistration> GetRegistrationsForMessageType<TMessage>()
    {
        CleanupDeadActions();
        var messageType = typeof(TMessage);
        if (_registrationsForMessages.TryGetValue(messageType, out var value))
        {
            return value;
        }

        value = new ConcurrentList<IMessengerRegistration>();
        _registrationsForMessages[messageType] = value;

        return value;
    }

    private void CleanupDeadActions()
    {
        RemoveRegistrations(x => !x.IsAlive);
    }

    private void RemoveRegistrations(Predicate<IMessengerRegistration> predicate)
    {
        foreach (var receiver in _registrationsForMessages)
        {
            var registrations = receiver.Value;
            var registrationsForRemove = registrations.Where(x => predicate(x)).ToList();
            foreach (var registration in registrationsForRemove)
            {
                registration.RemovedFromMessenger();
                registrations.Remove(registration);
            }
        }
    }

    public IDisposable Register<TMessage>(object receiver, Action<TMessage> action)
    {
        return Register(receiver, action, KeepOwnerAliveMode.NotKeepAlive);
    }

    public IDisposable Register<TMessage>(object receiver, Action<TMessage> action,
        KeepOwnerAliveMode keepOwnerAliveMode)
    {
        var actions = GetRegistrationsForMessageType<TMessage>();
        var registration = new MessengerRegistration<TMessage>(this, receiver, action, keepOwnerAliveMode);
        actions.Add(registration);
        return registration;
    }

    public void Unregister(object receiver)
    {
        RemoveRegistrations(x => x.Target == receiver);
    }

    public void Unregister<TMessage>(object receiver)
    {
        var registrations = GetRegistrationsForMessageType<TMessage>();
        var registrationsForRemove = registrations.Where(x => x.Target == receiver).ToList();
        foreach (var registration in registrationsForRemove)
        {
            registration.RemovedFromMessenger();
            registrations.Remove(registration);
        }
    }

    public void Send<TMessage>(TMessage message)
    {
        Ensure.IsNotNull(message, nameof(message));

        var actions = GetRegistrationsForMessageType<TMessage>();

        actions.ForEach(action => action.Execute(message));
    }
}
