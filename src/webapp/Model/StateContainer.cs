using Shared.Dtos;

namespace webapp.Model
{
    public class StateContainer
    {
        private List<DeviceDto> devices = new();
        private List<SensorDto> sensors = new();
        private List<DeviceSensorList> availableSensors = new(); 
        private List<List<DeviceSensorDto>> sensorCollection = new();

        public List<DeviceDto> Devices
        {
            get => devices;
            set
            {
                devices = value;
                NotifyStateChanged();
            }
        }

        public List<SensorDto> Sensors
        {
            get => sensors;
            set
            {
                sensors = value;
                NotifyStateChanged();
            }
        }

        public List<DeviceSensorList> AvailableSensors
        {
            get => availableSensors;
            set
            {
                availableSensors = value;
                NotifyStateChanged();
            }
        }

        public List<List<DeviceSensorDto>> SensorCollection
        {
            get => sensorCollection;
            set
            {
                sensorCollection = value;
                NotifyStateChanged();
            }
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
