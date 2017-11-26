using FriendOrganizer.UI.Data.Repositories;
using FriendOrganizer.UI.View.Services;
using FriendOrganizer.UI.Wrapper;
using Prism.Events;
using System;
using System.Threading.Tasks;
using FriendOrganizer.Model;
using Prism.Commands;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;
using FriendOrganizer.UI.Event;
using FriendOrganizer.DataAccess;

namespace FriendOrganizer.UI.ViewModel
{
    public class MeetingDetailViewModel : DetailViewModelBase, IMeetingDetailViewModel
    {
        private MeetingWrapper _meeting;
        private IWeatherService _weatherService;
        private IMeetingRepository _meetingRepository;
        private Friend _selectedAvailableFriend;
        private Friend _selectedAddedFriend;
        private List<Friend> _allFriends;

        public MeetingDetailViewModel(IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService,
            IWeatherService weatherService,
            IMeetingRepository meetingRepository)
            : base(eventAggregator, messageDialogService)
        {
            _weatherService = weatherService;
            _meetingRepository = meetingRepository;
            _eventAggregator.GetEvent<AfterDetailSavedEvent>().Subscribe(AfterDetailSaved);
            _eventAggregator.GetEvent<AfterDetailDeletedEvent>().Subscribe(AfterDetailDeleted);

            AddedFriends = new ObservableCollection<Friend>();
            AvailableFriends = new ObservableCollection<Friend>();
            AddFriendCommand = new DelegateCommand(OnAddFriendExecute, OnAddFriendCanExecute);
            RemoveFriendCommand = new DelegateCommand(OnRemoveFriendExecute, OnRemoveFriendCanExecute);
            RefreshWeatherCommand = new DelegateCommand(OnRefreshWeatherExecute);
        }

        private async void OnRefreshWeatherExecute()
        {
            var forecast = _weatherService.GetWeather();

            if (forecast != null)
            {
                WeatherForecast = forecast;
            }
            else
            {
                await _messageDialogService.ShowInfoDialogAsync("API access failed. \nWeather forecast has not been updated."); 
            }
        }

        private async void AfterDetailSaved(AfterDetailSavedEventArgs args)
        {
            if (args.ViewModelName == nameof(FriendDetailViewModel))
            {
                await _meetingRepository.ReloadFriendAsync(args.Id);
                _allFriends = await _meetingRepository.GetAllFriendsAsync();
                SetupPicklist();
            }
        }

        private async void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            if (args.ViewModelName == nameof(FriendDetailViewModel))
            {
                _allFriends = await _meetingRepository.GetAllFriendsAsync();
                SetupPicklist();
            }
        }

        public ICommand AddFriendCommand { get; }

        public ICommand RemoveFriendCommand { get; }

        public ICommand RefreshWeatherCommand { get; }

        private Dictionary<int, WeatherInfo> _weatherForecast;

        public Dictionary<int, WeatherInfo> WeatherForecast
        {
            get { return _weatherForecast; }
            set
            {
                if (value != null)
                {
                    _weatherForecast = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<Friend> AddedFriends { get; }

        public ObservableCollection<Friend> AvailableFriends { get; }

        public Friend SelectedAvailableFriend
        {
            get { return _selectedAvailableFriend; }
            set
            {
                _selectedAvailableFriend = value;
                OnPropertyChanged();
                ((DelegateCommand)AddFriendCommand).RaiseCanExecuteChanged();
            }
        }

        public Friend SelectedAddedFriend
        {
            get { return _selectedAddedFriend; }
            set
            {
                _selectedAddedFriend = value;
                OnPropertyChanged();
                ((DelegateCommand)RemoveFriendCommand).RaiseCanExecuteChanged();
            }
        }

        public MeetingWrapper Meeting
        {
            get { return _meeting; }
            private set
            {
                _meeting = value;
                OnPropertyChanged();
            }
        }

        public async override Task LoadAsync(int meetingId)
        {
            var meeting = meetingId > 0
                ? await _meetingRepository.GetByIdAsync(meetingId)
                : CreateNewMeeting();

            Id = meetingId;

            InitializeMeeting(meeting);

            _allFriends = await _meetingRepository.GetAllFriendsAsync();

            SetupPicklist();
            OnRefreshWeatherExecute();
        }

        private void SetupPicklist()
        {
            var meetingFriendIds = Meeting.Model.Friends.Select(f => f.Id).ToList();
            var addedFriends = _allFriends.Where(f => meetingFriendIds.Contains(f.Id)).OrderBy(f => f.FirstName);
            var availableFriends = _allFriends.Except(addedFriends).OrderBy(f => f.FirstName);

            AddedFriends.Clear();
            AvailableFriends.Clear();

            foreach (var addedFriend in addedFriends)
            {
                AddedFriends.Add(addedFriend);
            }

            foreach (var availableFriend in availableFriends)
            {
                AvailableFriends.Add(availableFriend);
            }
        }

        private bool OnAddFriendCanExecute()
        {
            return SelectedAvailableFriend != null;
        }

        private void OnAddFriendExecute()
        {
            var friendToAdd = SelectedAvailableFriend;

            Meeting.Model.Friends.Add(friendToAdd);
            AddedFriends.Add(friendToAdd);
            AvailableFriends.Remove(friendToAdd);
            HasChanges = _meetingRepository.HasChanges();
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private bool OnRemoveFriendCanExecute()
        {
            return SelectedAddedFriend != null;
        }

        private void OnRemoveFriendExecute()
        {
            var friendToRemove = SelectedAddedFriend;

            Meeting.Model.Friends.Remove(friendToRemove);
            AddedFriends.Remove(friendToRemove);
            AvailableFriends.Add(friendToRemove);
            HasChanges = _meetingRepository.HasChanges();
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        protected async override void OnDeleteExecute()
        {
            var result = await _messageDialogService.ShowOkCancelDialogAsync($"Do you really want to delete the meeting {Meeting.Title}?", "Question");

            if (result == MessageDialogResult.OK)
            {
                _meetingRepository.Remove(Meeting.Model);
                await _meetingRepository.SaveAsync();
                RaiseDetailDeletedEvent(Meeting.Id);
            }
        }

        protected override bool OnSaveCanExecute()
        {
            return Meeting != null && !Meeting.HasErrors && HasChanges;
        }

        protected async override void OnSaveExecute()
        {
            await SaveWithOptimisticConcurrencyAsync(_meetingRepository.SaveAsync,
                () =>
                {
                    HasChanges = _meetingRepository.HasChanges();
                    Id = Meeting.Id;
                    RaiseDetailSavedEvent(Meeting.Id, Meeting.Title);
                });
        }

        private Meeting CreateNewMeeting()
        {
            var meeting = new Meeting
            {
                DateFrom = DateTime.Now.Date,
                DateTo = DateTime.Now.Date
            };

            _meetingRepository.Add(meeting);
            return meeting;
        }

        private void InitializeMeeting(Meeting meeting)
        {
            Meeting = new MeetingWrapper(meeting);

            Meeting.PropertyChanged += (s, e) =>
            {
                if (!HasChanges)
                {
                    HasChanges = _meetingRepository.HasChanges();
                }

                if (e.PropertyName == nameof(Meeting.HasErrors))
                {
                    ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
                }

                if (e.PropertyName == nameof(Meeting.Title))
                {
                    SetTitle();
                }
            };

            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();

            if (Meeting.Id == 0)
            {
                // Little trick to trigger the validation
                Meeting.Title = "";
            }

            SetTitle();
        }

        private void SetTitle()
        {
            Title = Meeting.Title;
        }
    }
}
