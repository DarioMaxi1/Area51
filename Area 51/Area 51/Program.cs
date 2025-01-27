using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public enum SecurityLevel
{
    Confidential,
    Secret,
    TopSecret
}

public enum Floor
{
    G,
    S,
    T1,
    T2
}

public class Agent
{
    public SecurityLevel Security { get; private set; }
    public Floor CurrentFloor { get; private set; }

    public Agent(SecurityLevel security, Floor startFloor)
    {
        Security = security;
        CurrentFloor = startFloor;
    }

    public void MoveToFloor(Floor newFloor)
    {
        Console.WriteLine($"Agent with {Security} access is moving from {CurrentFloor} to {newFloor}.");
        CurrentFloor = newFloor;
    }
}

public class Elevator
{
    private Floor _currentFloor = Floor.G;
    private bool[] _buttonsEnabled = { true, true, true, true };
    private readonly object _lock = new object();

    private const int ElevatorSpeed = 1000;

    public Floor CurrentFloor => _currentFloor;

    public void CallElevator(Floor destination, Agent agent)
    {
        Console.WriteLine($"Elevator called by agent from {agent.CurrentFloor} to {destination}.");

        lock (_lock)
        {
            DisableAllButtons();
            MoveElevatorTo(destination, agent);
        }
    }

    private void MoveElevatorTo(Floor destination, Agent agent)
    {
        Console.WriteLine($"Elevator is moving to {destination}...");

        while (_currentFloor != destination)
        {
            Thread.Sleep(ElevatorSpeed);

            _currentFloor = (Floor)(((int)_currentFloor + 1) % 4);
            Console.WriteLine($"Elevator is now at {_currentFloor}");
        }

        OpenDoor(agent, destination);
    }

    public void OpenDoor(Agent agent, Floor requestedFloor)
    {
        if (CanAccessFloor(agent.Security, requestedFloor))
        {
            Console.WriteLine($"Elevator door opens at {requestedFloor} for agent with {agent.Security} access.");
            agent.MoveToFloor(requestedFloor);
        }
        else
        {
            Console.WriteLine($"Agent with {agent.Security} cannot access floor {requestedFloor}. Door stays closed.");
            agent.MoveToFloor(Floor.G);
            Console.WriteLine($"Agent is moved back to the ground floor and will try again.");
            CallElevator(Floor.G, agent);
        }

        EnableAllButtons();
    }

    private bool CanAccessFloor(SecurityLevel securityLevel, Floor floor)
    {
        return floor switch
        {
            Floor.G => true,
            Floor.S => securityLevel >= SecurityLevel.Secret,
            Floor.T1 => securityLevel == SecurityLevel.TopSecret,
            Floor.T2 => securityLevel == SecurityLevel.TopSecret,
            _ => false,
        };
    }

    private void DisableAllButtons()
    {
        for (int i = 0; i < _buttonsEnabled.Length; i++)
        {
            _buttonsEnabled[i] = false;
        }
    }

    private void EnableAllButtons()
    {
        for (int i = 0; i < _buttonsEnabled.Length; i++)
        {
            _buttonsEnabled[i] = true;
        }
    }
}

public class ElevatorButton
{
    public Floor Floor { get; private set; }

    public ElevatorButton(Floor floor)
    {
        Floor = floor;
    }

    public void PressButton(Elevator elevator, Agent agent)
    {
        Console.WriteLine($"Agent on floor {agent.CurrentFloor} presses the elevator button to go to {Floor}.");
        elevator.CallElevator(Floor, agent);
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        Elevator elevator = new Elevator();
        List<Agent> agents = new List<Agent>
        {
            new Agent(SecurityLevel.Confidential, Floor.G),
            new Agent(SecurityLevel.Secret, Floor.S),
            new Agent(SecurityLevel.TopSecret, Floor.T1)
        };

        List<ElevatorButton> buttons = new List<ElevatorButton>
        {
            new ElevatorButton(Floor.G),
            new ElevatorButton(Floor.S),
            new ElevatorButton(Floor.T1),
            new ElevatorButton(Floor.T2)
        };

        List<Task> agentTasks = new List<Task>();
        foreach (var agent in agents)
        {
            agentTasks.Add(Task.Run(() =>
            {
                Random rand = new Random();
                int targetFloor = rand.Next(0, 4);
                Floor target = (Floor)targetFloor;

                buttons[targetFloor].PressButton(elevator, agent);
            }));
        }

        await Task.WhenAll(agentTasks);

        Console.WriteLine("Simulation complete.");
    }
}
