﻿using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using CreativeCoders.Core.Threading;
using FluentAssertions;
using Xunit;

namespace CreativeCoders.Core.UnitTests.Threading;

public class SynchronizedValueTests
{
    [Fact]
    public void Value_SetViaCtor_ReturnsExpectedValue()
    {
        const int expectedValue = 12345;

        // Arrange & Act
        var value = SynchronizedValue.Create<int>();
        value.Value = expectedValue;

        // Assert
        value.Value
            .Should()
            .Be(expectedValue);
    }

    [Fact]
    public void Value_SetViaCtorWithLock_ReturnsExpectedValue()
    {
        const int expectedValue = 12345;

        // Arrange & Act
        var value = SynchronizedValue.Create<int>(new LockLockingMechanism());
        value.Value = expectedValue;

        // Assert
        value.Value
            .Should()
            .Be(expectedValue);
    }

    [Fact]
    public void SetValue_SetValue_ReturnsExpectedValue()
    {
        const int oldValue = 54321;
        const int expectedValue = 12345;

        // Arrange
        var value = SynchronizedValue.Create(oldValue);

        // Act
        value.SetValue(x =>
        {
            x
                .Should()
                .Be(oldValue);

            return expectedValue;
        });

        // Assert
        value.Value
            .Should()
            .Be(expectedValue);
    }

    [Fact]
    public void Value_SetValue_ReturnsExpectedValue()
    {
        const int oldValue = 54321;
        const int expectedValue = 12345;

        // Arrange
        var value = SynchronizedValue.Create(oldValue);

        // Act
        value.Value = expectedValue;

        // Assert
        value.Value
            .Should()
            .Be(expectedValue);
    }
#nullable enable
    [Fact]
    [SuppressMessage("csharpsquid", "S2925")]
    public void SetValue_ParallelReadValue_ValueAfterSetIsReturned()
    {
        const int expectedValue = 12345;

        // Arrange
        var synchronizedValue = SynchronizedValue.Create<int>(new LockSlimLockingMechanism());

        var readValue = 0;
        long readTimeMs = 0;

        // Act
        var setTask = new Thread(async () =>
        {
            synchronizedValue.SetValue(_ =>
            {
                Thread.Sleep(1000);

                return expectedValue;
            });
        });

        setTask.Start();

        var getTask = new Thread(() =>
        {
            Thread.Sleep(100);

            var stopwatch = Stopwatch.StartNew();

            readValue = synchronizedValue.Value;

            stopwatch.Stop();

            readTimeMs = stopwatch.ElapsedMilliseconds;
        });

        getTask.Start();

        setTask.Join();

        getTask.Join();

        // Assert
        readValue
            .Should()
            .Be(expectedValue);

        readTimeMs
            .Should()
            .BeGreaterThan(600);
    }
}
