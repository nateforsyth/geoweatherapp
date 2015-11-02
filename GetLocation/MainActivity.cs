using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Locations;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using WundergroundNetLib.Interfaces;
using WundergroundNetLib;

namespace GetLocation
{
    [Activity(Label = "GetLocation", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ILocationListener
    {
        Location _currentLocation;
        LocationManager _locationManager;
        TextView _locationText;
        TextView _addressText;
        EditText latInput;
        EditText longInput;
        TextView _weatherOutput;
        String _locationProvider;
        Button _btnGetAddress;
        Button _btnGetWeather;

        // collections
        List<Location> visitedLocations;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _addressText = FindViewById<TextView>(Resource.Id.address_text);
            _locationText = FindViewById<TextView>(Resource.Id.location_text);
            _btnGetAddress = FindViewById<Button>(Resource.Id.btnGetAddress);
            _btnGetAddress.Click += AddressButton_OnClick;

            _weatherOutput = FindViewById<TextView>(Resource.Id.txtWeatherText);
            _btnGetWeather = FindViewById<Button>(Resource.Id.btnGetWeather);
            _btnGetWeather.Click += GetWeatherFromLatLong_OnClick;

            InitialiseLocationManager();
        }

        async void GetWeatherFromLatLong_OnClick(object sender, EventArgs e)
        {
            if (_currentLocation == null)
            {
                _weatherOutput.Text = "Can't determine current coordinates.";
                return;
            }

            try
            {
                Geocoder geocoder = new Geocoder(this);
                IList<Address> addressList = await geocoder.GetFromLocationAsync(_currentLocation.Latitude, _currentLocation.Longitude, 10);

                Address address = addressList.FirstOrDefault();

                if (address != null)
                {
                    IDataProvider dataProvider = DataProvider.DefaultProvider; // ensure only one instance is created using the DataProvider singleton

                    var WeatherData = dataProvider.GetCombinedDataAsync(address.Latitude, address.Longitude);

                    string weatherString = string.Format("--------------------------------------\r\n" +
                                                            "Observation Location Details\r\n" +
                                                            "City \t\t\t{0}\r\n" +
                                                            "Country \t\t{1}\r\n" +
                                                            "StationLatitude \t{2}\r\n" +
                                                            "StationLongitude \t{3}\r\n" +
                                                            "StationElevation \t{4}\r\n" +
                                                            "StationID \t\t{5}\r\n" +
                                                            "WmoNumber \t\t{6}\r\n",
                                                            WeatherData.Result.observationLocation.City,
                                                            WeatherData.Result.observationLocation.Country,
                                                            WeatherData.Result.observationLocation.StationLatitude,
                                                            WeatherData.Result.observationLocation.StationLongitude,
                                                            WeatherData.Result.observationLocation.StationElevation,
                                                            WeatherData.Result.observationLocation.StationID,
                                                            WeatherData.Result.observationLocation.WmoNumber);

                    _weatherOutput.Text = weatherString;
                }
                else
                {
                    _weatherOutput.Text = "Can't determine current location.";
                }
            }
            catch (Exception ex)
            {
                _weatherOutput.Text = ex.Message;
            }
        }

        private void InitialiseLocationManager()
        {
            _locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria{ Accuracy = Accuracy.Fine };
            IList<string> acceptableLocationProviders = _locationManager.GetProviders(criteriaForLocationService, true);

            if (acceptableLocationProviders.Any())
            {
                _locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                _locationProvider = String.Empty;
            }
        }

        async void AddressButton_OnClick(object sender, EventArgs e)
        {
            if (_currentLocation == null)
            {
                _addressText.Text = "Can't determine current address.";
                return;
            }

            Geocoder geocoder = new Geocoder(this);
            IList<Address> addressList = await geocoder.GetFromLocationAsync
                                        (_currentLocation.Latitude, _currentLocation.Longitude, 10);

            Address address = addressList.FirstOrDefault();
            if (address != null)
            {
                StringBuilder deviceAddress = new StringBuilder();
                for (int i = 0; i < address.MaxAddressLineIndex; i++)
                {
                    deviceAddress.Append(address.GetAddressLine(i)).AppendLine(",");
                }
                _addressText.Text = deviceAddress.ToString();
            }
            else
            {
                _addressText.Text = "Unable to determine the address.";
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            _locationManager.RequestLocationUpdates(_locationProvider, 0, 0, this);
        }

        protected override void OnPause()
        {
            base.OnPause();
            _locationManager.RemoveUpdates(this);
        }

        public void OnLocationChanged(Location location)
        {
            _currentLocation = location;
            if (_currentLocation == null)
            {
                _locationText.Text = "Unable to determine your location";
            }
            else
            {
                _locationText.Text = String.Format("{0},{1}", _currentLocation.Latitude, _currentLocation.Longitude);
            }
        }

        public void OnProviderDisabled(string provider)
        {
            throw new NotImplementedException();
        }

        public void OnProviderEnabled(string provider)
        {
            throw new NotImplementedException();
        }

        public void OnStatusChanged(string provider, [GeneratedEnum]Availability status, Bundle extras)
        {
            throw new NotImplementedException();
        }
    }
}

