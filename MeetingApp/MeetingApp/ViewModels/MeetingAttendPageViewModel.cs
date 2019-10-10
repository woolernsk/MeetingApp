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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MeetingApp.ViewModels
{
    public class MeetingAttendPageViewModel : ViewModelBase
    {
        private MeetingData _targetMeetingData;
        private ObservableCollection<MeetingLabelData> _targetMeetingLabels;
        private string _inputLabelItemName;
        private int _labelheight;

        private CreateMeetingLabelItemValidation _createMeetingLabelItemValidation;

        private TokenCheckParam _tokenCheckParam;
        private GetUserParam _getUserParam;
        private GetMeetingParam _getMeetingParam;
        private CreateMeetingLabelItemParam _createMeetingLabelItemParam;
        private GetMeetingLabelsParam _getMeetingLabelsParam;

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
        public ObservableCollection<MeetingLabelData> TargetMeetingLabels
        {
            get { return _targetMeetingLabels; }
            set { SetProperty(ref _targetMeetingLabels, value); }
        }

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
        public int LabelHeight
        {
            get { return _labelheight; }
            set { SetProperty(ref _labelheight, value); }
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


        public ICommand CreateMeetingLabelItemCommand { get; }
        public ICommand EnterMeetingCommand { get; }
        public ICommand ExitMeetingCommand { get; }

        public ICommand NavigateMeetingLabelItemDataCreatePage { get; }

        INavigationService _navigationService;

        RestService _restService;
        TokenCheckValidation _tokenCheckValidation;
        ApplicationProperties _applicationProperties;


        public MeetingAttendPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            _createMeetingLabelItemParam = new CreateMeetingLabelItemParam();
            _createMeetingLabelItemValidation = new CreateMeetingLabelItemValidation();
            _navigationService = navigationService;
            _tokenCheckValidation = new TokenCheckValidation();
            _applicationProperties = new ApplicationProperties();


            //会議の各ラベルに項目（Item)を追加するコマンド
            CreateMeetingLabelItemCommand = new DelegateCommand<object>(async (id) =>
            {
                var lid = Convert.ToInt32(id);
                var uid = 0;

                //項目入力値のバリデーション
                CreateMeetingLabelItemParam = _createMeetingLabelItemValidation.InputValidate(InputLabelItemName);
                if (CreateMeetingLabelItemParam.HasError == true) { return; }


                //項目を追加する先のリストを特定し追加

                //uid取得の際のtoken情報照合
                TokenCheckParam = await _tokenCheckValidation.Validate();

                if (TokenCheckParam.HasError == true)
                {
                    return;
                }
                else
                {
                    //token情報照合に成功したらuid取得
                    GetUserParam = await _restService.GetUserDataAsync(UserConstants.OpenUserEndPoint, _applicationProperties.GetFromProperties<string>("userId"));

                    if (GetUserParam.HasError == true)
                    {
                        return;
                    }
                    else
                    {
                        //userDataの取得に成功したらuidを代入
                        var userData = GetUserParam.User;
                        uid = userData.Id;

                    }

                }

                var meetingLabelItemData = new MeetingLabelItemData(lid, uid, InputLabelItemName);
                TargetMeetingLabels.FirstOrDefault(l => l.Id == lid).MeetingLabelItemDatas.Add(meetingLabelItemData);

                InputLabelItemName = "";

            });


            //ラベルに項目を追加するページへ遷移するコマンド
            NavigateMeetingLabelItemDataCreatePage = new DelegateCommand<object>((meetingLabelData) =>
            {
                var targetMeetingLabelData = (MeetingLabelData)(meetingLabelData);


                var p = new NavigationParameters
                {
                    { "meetingLabelData", targetMeetingLabelData}
                };
                _navigationService.NavigateAsync("MeetingLabelItemDataCreatePage");
            });


            //会議に入室するコマンド
            EnterMeetingCommand = new DelegateCommand(async () =>
            {
                //バリデーション
                //CreateMeetingLabelItemParam = _createMeetingLabelItemValidation.InputValidate();

                //ラベルアイテムをDBにInsert
                foreach (MeetingLabelData l in TargetMeetingLabels)
                {
                    foreach (MeetingLabelItemData i in l.MeetingLabelItemDatas)
                    {
                        CreateMeetingLabelItemParam = await _restService.CreateMeetingLabelItemDataAsync(MeetingConstants.OPENMeetingLabelItemEndPoint, i);
                    }
                }

                if (CreateMeetingLabelItemParam.IsSuccessed == true)
                {
                    await _navigationService.NavigateAsync("MeetingExecuteTopPage");
                }

            });

            //会議入室画面から退室するコマンド
            ExitMeetingCommand = new DelegateCommand(() =>
            {
                _navigationService.NavigateAsync("MeetingDataTopPage");


            });


        }

        public override async void OnNavigatingTo(INavigationParameters parameters)
        {
            base.OnNavigatingTo(parameters);

            _restService = new RestService();
            _getMeetingLabelsParam = new GetMeetingLabelsParam();
            _getMeetingParam = new GetMeetingParam();

            //対象の会議データ取得
            GetMeetingParam = await _restService.GetMeetingDataAsync(MeetingConstants.OpenMeetingEndPoint, (int)parameters["mid"]);
            TargetMeetingData = GetMeetingParam.MeetingData;

            //会議のラベルを取得
            GetMeetingLabelsParam = await _restService.GetMeetingLabelsDataAsync(MeetingConstants.OPENMeetingLabelEndPoint, (int)parameters["mid"]);
            TargetMeetingLabels = new ObservableCollection<MeetingLabelData>(GetMeetingLabelsParam.MeetingLabelDatas);

        }
    }
}
