﻿    using FriendOrganizer.UI.Data.Repositories;
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
        private IDetailViewModel _detailViewModel;
        private IEventAggregator _eventAggregator;

        public MainViewModel(IFriendRepository friendDataService, INavigationViewModel navigationViewModel, 
            Func<IFriendDetailViewModel> friendDetailViewModelCreator, IEventAggregator eventAggregator, 
            IMessageDialogService messageDialogService)
        {
            _friendDetailViewModelCreator = friendDetailViewModelCreator;
            _messageDialogService = messageDialogService;

            _eventAggregator = eventAggregator;
            
            _eventAggregator.GetEvent<OpenDetailViewEvent>()
                .Subscribe(OnOpenDetailView);

            _eventAggregator.GetEvent<AfterDetailDeletedEvent>()
                .Subscribe(AfterDetailDeleted);

            CreateNewDetailCommand = new DelegateCommand<Type>(OnCreateNewDetailExecute);

            NavigationViewModel = navigationViewModel;
        }
        
        private void OnCreateNewDetailExecute(Type viewModelType)
        {
            OnOpenDetailView(
                new OpenDetailViewEventArgs
            {
                    ViewModelName = viewModelType.Name
            });
        }

        public ICommand CreateNewDetailCommand { get; }

        public INavigationViewModel NavigationViewModel { get; }

        public IDetailViewModel DetailViewModel
        {
            get { return _detailViewModel; }
            private set
            {
                _detailViewModel = value;
                OnPropertyChanged();
            }
        }

        public async Task LoadAsync()
        {
            await NavigationViewModel.LoadAsync();
        }

        private async void OnOpenDetailView(OpenDetailViewEventArgs args)
        {
            if ((DetailViewModel != null) && (DetailViewModel.HasChanges))
            {
                var result = _messageDialogService.ShowOkCancelDialog("You've made changes. Navigate away?", "Question");

                if (result == MessageDialogResult.Cancel)
                {
                    return;
                }
            }

            switch (args.ViewModelName)
            {
                case nameof(FriendDetailViewModel):
                    DetailViewModel = _friendDetailViewModelCreator();
                    break;
            }

            await DetailViewModel.LoadAsync(args.Id);
        }

        private void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            DetailViewModel = null;
        }
    }
}
