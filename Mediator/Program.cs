using System;
using System.Collections.Generic;
using System.Linq;

namespace DesignPatterns.Mediator
{
    public interface ICommandCentre
    {
        void RequestLanding(Aircraft aircraft);
        void RequestTakeOff(Aircraft aircraft);
    }

    public class Aircraft
    {
        public string Name { get; private set; }
        public Guid? CurrentRunwayId { get; private set; }
        public bool IsTakingOff { get; set; }
        private ICommandCentre _commandCentre;

        public Aircraft(string name, ICommandCentre commandCentre)
        {
            this.Name = name;
            this._commandCentre = commandCentre;
        }

        public void RequestLanding()
        {
            Console.WriteLine($"Aircraft {this.Name} requests permission to land.");
            _commandCentre.RequestLanding(this);
        }

        public void RequestTakeOff()
        {
            Console.WriteLine($"Aircraft {this.Name} requests permission to take off.");
            _commandCentre.RequestTakeOff(this);
        }

        public void NotifyLandingApproved(Guid runwayId)
        {
            this.CurrentRunwayId = runwayId;
            Console.WriteLine($"Aircraft {this.Name} has landed on runway {runwayId}.");
        }

        public void NotifyLandingDenied()
        {
            Console.WriteLine($"Aircraft {this.Name} landing request denied - no available runways.");
        }

        public void NotifyTakeOffApproved()
        {
            this.CurrentRunwayId = null;
            Console.WriteLine($"Aircraft {this.Name} has taken off successfully.");
        }

        public void NotifyTakeOffDenied()
        {
            Console.WriteLine($"Aircraft {this.Name} takeoff request denied - not currently on a runway.");
        }
    }
 
    public class Runway
    {
        public readonly Guid Id = Guid.NewGuid();
        private Aircraft _currentAircraft;
        private ICommandCentre _commandCentre;

        public Runway(ICommandCentre commandCentre)
        {
            this._commandCentre = commandCentre;
            this._currentAircraft = null;
        }

        public bool IsAvailable
        {
            get { return _currentAircraft == null; }
        }

        public bool CheckIsActive()
        {
            if (_currentAircraft != null)
            {
                return _currentAircraft.IsTakingOff;
            }
            return false;
        }

        public bool AssignAircraft(Aircraft aircraft)
        {
            if (IsAvailable)
            {
                _currentAircraft = aircraft;
                HighLightRed();
                return true;
            }
            return false;
        }

        public bool ReleaseAircraft(Aircraft aircraft)
        {
            if (_currentAircraft == aircraft)
            {
                _currentAircraft = null;
                HighLightGreen();
                return true;
            }
            return false;
        }

        public Aircraft GetCurrentAircraft()
        {
            return _currentAircraft;
        }

        private void HighLightRed()
        {
            Console.WriteLine($"Runway {this.Id} is now busy!");
        }

        private void HighLightGreen()
        {
            Console.WriteLine($"Runway {this.Id} is now free!");
        }
    }

    public class CommandCentre : ICommandCentre
    {
        private List<Runway> _runways = new List<Runway>();
        private List<Aircraft> _aircrafts = new List<Aircraft>();

        public CommandCentre()
        {
 
        }

        public void AddRunway(Runway runway)
        {
            _runways.Add(runway);
        }

        public void AddAircraft(Aircraft aircraft)
        {
            _aircrafts.Add(aircraft);
        }

        public void RequestLanding(Aircraft aircraft)
        {
            Console.WriteLine($"Command Centre processing landing request from {aircraft.Name}");

            Runway availableRunway = null;
            foreach (var runway in _runways)
            {
                if (runway.IsAvailable)
                {
                    availableRunway = runway;
                    break;
                }
            }

            if (availableRunway != null)
            {
                Console.WriteLine($"Command Centre: Assigning runway {availableRunway.Id} to aircraft {aircraft.Name}");
                availableRunway.AssignAircraft(aircraft);
                aircraft.NotifyLandingApproved(availableRunway.Id);
            }
            else
            {
                Console.WriteLine($"Command Centre: No available runways for aircraft {aircraft.Name}");
                aircraft.NotifyLandingDenied();
            }
        }

        public void RequestTakeOff(Aircraft aircraft)
        {
            Console.WriteLine($"Command Centre processing takeoff request from {aircraft.Name}");

            if (aircraft.CurrentRunwayId.HasValue)
            {
                Runway runway = null;
                foreach (var r in _runways)
                {
                    if (r.Id == aircraft.CurrentRunwayId.Value)
                    {
                        runway = r;
                        break;
                    }
                }

                if (runway != null && runway.GetCurrentAircraft() == aircraft)
                {
                    Console.WriteLine($"Command Centre: Approving takeoff for aircraft {aircraft.Name} from runway {runway.Id}");
                    runway.ReleaseAircraft(aircraft);
                    aircraft.NotifyTakeOffApproved();
                }
                else
                {
                    Console.WriteLine($"Command Centre: Cannot approve takeoff - aircraft {aircraft.Name} not properly assigned to runway");
                    aircraft.NotifyTakeOffDenied();
                }
            }
            else
            {
                Console.WriteLine($"Command Centre: Cannot approve takeoff - aircraft {aircraft.Name} is not on any runway");
                aircraft.NotifyTakeOffDenied();
            }
        }

        public void ShowStatus()
        {
            Console.WriteLine("\n=== Airport Status ===");
            foreach (var runway in _runways)
            {
                string status = runway.IsAvailable ? "Available" : $"Occupied by {runway.GetCurrentAircraft().Name}";
                Console.WriteLine($"Runway {runway.Id}: {status}");
            }
            Console.WriteLine();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Airport Traffic Control System with Mediator Pattern ===\n");

            var commandCentre = new CommandCentre();

            var runway1 = new Runway(commandCentre);
            var runway2 = new Runway(commandCentre);

            commandCentre.AddRunway(runway1);
            commandCentre.AddRunway(runway2);

            var aircraft1 = new Aircraft("Boeing 737", commandCentre);
            var aircraft2 = new Aircraft("Airbus A320", commandCentre);
            var aircraft3 = new Aircraft("Cessna 172", commandCentre);

            commandCentre.AddAircraft(aircraft1);
            commandCentre.AddAircraft(aircraft2);
            commandCentre.AddAircraft(aircraft3);

            commandCentre.ShowStatus();

            aircraft1.RequestLanding();
            commandCentre.ShowStatus();

            aircraft2.RequestLanding();
            commandCentre.ShowStatus();

            aircraft3.RequestLanding();
            commandCentre.ShowStatus();

            aircraft1.RequestTakeOff();
            commandCentre.ShowStatus();

            aircraft3.RequestLanding();
            commandCentre.ShowStatus();

            var aircraft4 = new Aircraft("Boeing 777", commandCentre);
            aircraft4.RequestTakeOff();

            Console.WriteLine("\n=== End of Demonstration ===");
            Console.ReadKey();
        }
    }
}