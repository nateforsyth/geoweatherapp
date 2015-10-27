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
        TextView fakeAddress;
        String _locationProvider;

        // collections
        List<Location> visitedLocations;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _addressText = FindViewById<TextView>(Resource.Id.address_text);
            _locationText = FindViewById<TextView>(Resource.Id.location_text);
            FindViewById<TextView>(Resource.Id.get_address_button).Click += AddressButton_OnClick;

            // code to fake the address using user entered lat and long values
            latInput = FindViewById<EditText>(Resource.Id.txtLatInput);
            longInput = FindViewById<EditText>(Resource.Id.txtLongInput);
            fakeAddress = FindViewById<TextView>(Resource.Id.fake_address_text);
            FindViewById<TextView>(Resource.Id.btnFakeAddress).Click += FakeAddressButton_OnClick;

            InitialiseLocationManager();
        }

        async void FakeAddressButton_OnClick(object sender, EventArgs e)
        {
            string message = "";
            double lat;
            bool tryLat = double.TryParse(latInput.Text, out lat);
            double lon;
            bool tryLong = double.TryParse(longInput.Text, out lon);

            try
            {
                if (tryLat == false || tryLong == false)
                {
                    message = "Unable to determine the address or input is invalid.";
                }
                else
                {
                    Geocoder geocoder = new Geocoder(this);
                    IList<Address> addList = await geocoder.GetFromLocationAsync(lat, lon, 10);
                    Address add = addList.FirstOrDefault();

                    if (add != null)
                    {
                        StringBuilder deviceAddress = new StringBuilder();
                        for (int i = 0; i < add.MaxAddressLineIndex; i++)
                        {
                            deviceAddress.Append(add.GetAddressLine(i)).AppendLine(",");
                        }
                        fakeAddress.Text = deviceAddress.ToString();
                    }
                    else
                    {
                        fakeAddress.Text = string.Format("{0}", message);
                    }
                }
            }
            catch (Exception ex)
            {
                fakeAddress.Text = ex.Message;
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

