    using FriendOrganizer.UI.Data.Repositories;
using FriendOrganizer.UI.Event;
using FriendOrganizer.UI.View.Services;
using Prism.Commands;
using Prism.Events;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FriendOrganizer.UI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private Func<IFriendDetailViewModel> _friendDetailViewModelCreator;
        private IMessageDialogService _messageDialogService;
        private IFriendDetailViewModel _friendDetailViewModel;
        private IEventAggregator _eventAggregator;

        public MainViewModel(IFriendRepository friendDataService, INavigationViewModel navigationViewModel, 
            Func<IFriendDetailViewModel> friendDetailViewModelCreator, IEventAggregator eventAggregator, 
            IMessageDialogService messageDialogService)
        {
            _friendDetailViewModelCreator = friendDetailViewModelCreator;
            _messageDialogService = messageDialogService;

            _eventAggregator = eventAggregator;
            
            _eventAggregator.GetEvent<OpenFriendDetailViewEvent>()
                .Subscribe(OnOpenFriendDetailView);

            _eventAggregator.GetEvent<AfterFriendDeletedEvent>()
                .Subscribe(AfterFriendDeleted);

            CreateNewFriendCommand = new DelegateCommand(OnCreateNewFriendExecute);

            NavigationViewModel = navigationViewModel;
        }


        private void OnCreateNewFriendExecute()
        {
            OnOpenFriendDetailView(null);
        }

        public ICommand CreateNewFriendCommand { get; }

        public INavigationViewModel NavigationViewModel { get; }

        public IFriendDetailViewModel FriendDetailViewModel
        {
            get { return _friendDetailViewModel; }
            private set
            {
                _friendDetailViewModel = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadAsync()
        {
            await NavigationViewModel.LoadAsync();
        }

        private async void OnOpenFriendDetailView(int? friendId)
        {
            if ((FriendDetailViewModel != null) && (FriendDetailViewModel.HasChanges))
            {
                var result = _messageDialogService.ShowOkCancelDialog("You've made changes. Navigate away?", "Question");

                if (result == MessageDialogResult.Cancel)
                {
                    return;
                }
            }

            FriendDetailViewModel = _friendDetailViewModelCreator();
            await FriendDetailViewModel.LoadAsync(friendId);
        }

        private void AfterFriendDeleted(int friendId)
        {
            FriendDetailViewModel = null;
        }
    }
}
