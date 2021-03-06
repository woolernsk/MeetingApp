using MeetingApp.Constants;
using MeetingApp.Data;
using MeetingApp.Models.Constants;
using MeetingApp.Models.Data;
using MeetingApp.Models.Param;
using MeetingApp.Models.Validate;
using MeetingApp.Utils;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MeetingApp.ViewModels
{
    public class MeetingAttendPageViewModel : ViewModelBase
    {
        #region private data

        private MeetingData _targetMeetingData;
        private ObservableCollection<MeetingLabelData> _targetMeetingLabels;
        private string _inputLabelItemName;
        private int _labelItemCount;
        private bool _loadingData;

        #endregion

        #region private validation
        private CreateMeetingLabelItemValidation _createMeetingLabelItemValidation;
        private AttendMeetingValidation _attendMeetingValidation;
        #endregion

        #region private param

        private TokenCheckParam _tokenCheckParam;
        private GetUserParam _getUserParam;
        private GetMeetingParam _getMeetingParam;
        private CreateMeetingLabelItemParam _createMeetingLabelItemParam;
        private GetMeetingLabelsParam _getMeetingLabelsParam;
        private AttendMeetingParam _attendMeetingParam;
        private DeleteMeetingLabelItemParam _deleteMeetingLabelItemParam;
        private CreateParticipateParam _createParticipateParam;
        private CheckParticipantParam _checkParticipantParam;
        #endregion

        #region public data

        public MeetingData TargetMeetingData
        {
            get { return _targetMeetingData; }
            set { SetProperty(ref _targetMeetingData, value); }
        }
        public string InputLabelItemName
        {
            get { return _inputLabelItemName; }
            set { SetProperty(ref _inputLabelItemName, value); }
        }
        public bool LoadingData
        {
            get { return _loadingData; }
            set { SetProperty(ref _loadingData, value); }
        }
        public ObservableCollection<MeetingLabelData> TargetMeetingLabels
        {
            get { return _targetMeetingLabels; }
            set { SetProperty(ref _targetMeetingLabels, value); }
        }
        public int LabelItemCount
        {
            get { return _labelItemCount; }
            set { SetProperty(ref _labelItemCount, value); }
        }

        #endregion

        #region public param
        public CreateMeetingLabelItemParam CreateMeetingLabelItemParam
        {
            get { return _createMeetingLabelItemParam; }
            set { SetProperty(ref _createMeetingLabelItemParam, value); }
        }
        public GetMeetingLabelsParam GetMeetingLabelsParam
        {
            get { return _getMeetingLabelsParam; }
            set { SetProperty(ref _getMeetingLabelsParam, value); }
        }
        public GetMeetingParam GetMeetingParam
        {
            get { return _getMeetingParam; }
            set { SetProperty(ref _getMeetingParam, value); }
        }
        public TokenCheckParam TokenCheckParam
        {
            get { return _tokenCheckParam; }
            set { SetProperty(ref _tokenCheckParam, value); }
        }

        public GetUserParam GetUserParam
        {
            get { return _getUserParam; }
            set { SetProperty(ref _getUserParam, value); }
        }
        public AttendMeetingParam AttendMeetingParam
        {
            get { return _attendMeetingParam; }
            set { SetProperty(ref _attendMeetingParam, value); }
        }
        public DeleteMeetingLabelItemParam DeleteMeetingLabelItemParam
        {
            get { return _deleteMeetingLabelItemParam; }
            set { SetProperty(ref _deleteMeetingLabelItemParam, value); }
        }
        public CreateParticipateParam CreateParticipateParam
        {
            get { return _createParticipateParam; }
            set { SetProperty(ref _createParticipateParam, value); }
        }
        public CheckParticipantParam CheckParticipantParam
        {
            get { return _checkParticipantParam; }
            set { SetProperty(ref _checkParticipantParam, value); }
        }

        #endregion

        #region command

        public ICommand CreateMeetingLabelItemCommand { get; }
        public ICommand EnterMeetingCommand { get; }
        public ICommand ExitMeetingCommand { get; }

        public ICommand NavigateMeetingLabelItemDataCreatePage { get; }

        #endregion

        #region others

        INavigationService _navigationService;
        RestService _restService;
        TokenCheckValidation _tokenCheckValidation;
        ApplicationProperties _applicationProperties;
        OperateDateTime _operateDateTime;

        #endregion


        public MeetingAttendPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            _navigationService = navigationService;
            _restService = new RestService();
            _operateDateTime = new OperateDateTime();
            _applicationProperties = new ApplicationProperties();

            _createMeetingLabelItemParam = new CreateMeetingLabelItemParam();
            _attendMeetingParam = new AttendMeetingParam();
            _deleteMeetingLabelItemParam = new DeleteMeetingLabelItemParam();

            _createMeetingLabelItemValidation = new CreateMeetingLabelItemValidation();
            _tokenCheckValidation = new TokenCheckValidation(_restService);
            _attendMeetingValidation = new AttendMeetingValidation();


            //ラベルに関する項目を追加するページへ遷移するコマンド
            NavigateMeetingLabelItemDataCreatePage = new DelegateCommand<object>((meetingLabelData) =>
            {
                var targetMeetingLabelData = (MeetingLabelData)(meetingLabelData);

                var p = new NavigationParameters
                {
                    { "meetingLabelData", targetMeetingLabelData},
                };
                _navigationService.NavigateAsync("MeetingLabelItemDataCreatePage", p);

            });


            //会議に入室するコマンド
            EnterMeetingCommand = new DelegateCommand(async () =>
            {
                //バリデーション
                AttendMeetingParam = _attendMeetingValidation.ButtonPushedValidate(new List<MeetingLabelData>(TargetMeetingLabels));


                if (AttendMeetingParam.IsSuccessed == true)
                {
                    GetUserParam = await _restService.GetUserDataAsync(UserConstants.OpenUserEndPoint, _applicationProperties.GetFromProperties<string>("userId"));

                    var mid = GetMeetingParam.MeetingData.Id;
                    var uid = GetUserParam.User.Id;

                    //ParticipantDBに既にユーザーが居ないかチェック
                    CheckParticipantParam = await _restService.CheckParticipantDataAsync(MeetingConstants.OPENMeetingParticipantEndPoint, uid, mid);

                    //ユーザーが既にParticipantDBに存在していた場合
                    if (CheckParticipantParam.IsSuccessed == true)
                    {
                        //会議参加済みかつisDeletedがfalseの場合は最終更新時刻のみ更新し遷移する
                        if (CheckParticipantParam.Participant.isDeleted == false)
                        {
                            var operateDateTime = new OperateDateTime();
                            CheckParticipantParam.Participant.LastUpdateTime = operateDateTime.CurrentDateTime;
                            var updateParticipant = CheckParticipantParam.Participant;
                            await _restService.UpdateParticipantDataAsync(MeetingConstants.OPENMeetingParticipantEndPoint, updateParticipant);

                            var p = new NavigationParameters
                            {
                                { "mid", GetMeetingParam.MeetingData.Id}
                            };
                            await _navigationService.NavigateAsync("MeetingExecuteTopPage", p);

                        }
                        else
                        //参加済みかつ論理削除済みの場合はisDeletedをtrue→falseにして再入室
                        {
                            var operateDateTime = new OperateDateTime();
                            CheckParticipantParam.Participant.isDeleted = false;
                            CheckParticipantParam.Participant.LastUpdateTime = operateDateTime.CurrentDateTime;
                            var updateParticipant = CheckParticipantParam.Participant;

                            await _restService.UpdateParticipantDataAsync(MeetingConstants.OPENMeetingParticipantEndPoint, updateParticipant);

                            var p = new NavigationParameters
                            {
                                { "mid", GetMeetingParam.MeetingData.Id}
                            };
                            await _navigationService.NavigateAsync("MeetingExecuteTopPage", p);
                        }

                    }
                    //ParticipantDBに該当者が居なければ追加
                    else
                    {
                        CreateParticipateParam = await _restService.CreateParticipateDataAsync(MeetingConstants.OPENMeetingParticipantEndPoint, uid, mid);

                        if (CreateParticipateParam.IsSuccessed == true)
                        {
                            var p = new NavigationParameters
                            {
                                { "mid", GetMeetingParam.MeetingData.Id}
                            };
                            await _navigationService.NavigateAsync("MeetingExecuteTopPage", p);
                        }

                    }
                }

            });

            //会議入室画面から退室するコマンド
            ExitMeetingCommand = new DelegateCommand(async () =>
            {
                //TargetMeetingLabelsが所持しているItemsを削除する
                foreach (MeetingLabelData l in TargetMeetingLabels)
                {
                    foreach (MeetingLabelItemData i in l.MeetingLabelItemDatas)
                    {
                        DeleteMeetingLabelItemParam = await _restService.DeleteMeetingLabelItemDataAsync(MeetingConstants.OPENMeetingLabelItemEndPoint, i.Id);
                    }
                }

                await _navigationService.NavigateAsync("/NavigationPage/MeetingDataTopPage");


            });


        }

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);

            LoadingData = true;

            _restService = new RestService();
            _getMeetingLabelsParam = new GetMeetingLabelsParam();
            _getMeetingParam = new GetMeetingParam();

            //対象の会議データ取得
            GetMeetingParam = await _restService.GetMeetingDataAsync(MeetingConstants.OpenMeetingEndPoint, (int)parameters["mid"]);
            TargetMeetingData = GetMeetingParam.MeetingData;

            //UserDataを取得
            GetUserParam = await _restService.GetUserDataAsync(UserConstants.OpenUserEndPoint, _applicationProperties.GetFromProperties<string>("userId"));
            var uid = GetUserParam.User.Id;

            //会議のラベルを取得
            GetMeetingLabelsParam = await _restService.GetMeetingLabelsDataAsync(MeetingConstants.OPENMeetingLabelEndPoint, (int)parameters["mid"], uid);
            TargetMeetingLabels = new ObservableCollection<MeetingLabelData>(GetMeetingLabelsParam.MeetingLabelDatas);

            Console.WriteLine(TargetMeetingLabels);

            LoadingData = false;



        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {

            LoadingData = true;

            _restService = new RestService();
            _getMeetingLabelsParam = new GetMeetingLabelsParam();
            _getMeetingParam = new GetMeetingParam();

            //項目追加から戻ってきたときの更新処理
            //対象の会議データ取得
            GetMeetingParam = await _restService.GetMeetingDataAsync(MeetingConstants.OpenMeetingEndPoint, (int)parameters["mid"]);
            TargetMeetingData = GetMeetingParam.MeetingData;

            //UserDataを取得
            GetUserParam = await _restService.GetUserDataAsync(UserConstants.OpenUserEndPoint, _applicationProperties.GetFromProperties<string>("userId"));
            var uid = GetUserParam.User.Id;

            //会議のラベルを取得
            GetMeetingLabelsParam = await _restService.GetMeetingLabelsDataAsync(MeetingConstants.OPENMeetingLabelEndPoint, (int)parameters["mid"], uid);
            TargetMeetingLabels = new ObservableCollection<MeetingLabelData>(GetMeetingLabelsParam.MeetingLabelDatas);

            LoadingData = false;


        }

    }
}
