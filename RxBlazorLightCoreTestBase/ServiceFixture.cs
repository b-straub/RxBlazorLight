
using RxBlazorLightCore;
using System.Reactive;

namespace RxBlazorLightCoreTestBase
{
    public record CRUDTest(string Item, Guid Id);

    public partial class ServiceFixture : RxBLService
    {
        public IObservableStateProvider<Unit> ObservableStateVoidProvider { get; }
        public IObservableStateProvider<int> ObservableStateIntProvider { get; }

        public IState<int> ObservableIntState { get; }
        public IState<int> IntState { get; }
        public IState<int> IntStateAsyncX { get; }

        public IState<IEnumerable<CRUDTest>, List<CRUDTest>> CRUDListState { get; }
        public IState<IEnumerable<KeyValuePair<Guid, CRUDTest>>, Dictionary<Guid, CRUDTest>> CRUDDictState { get; }

        public IStateProvider<int> Increment { get; }
        public IStateTransformer<int> Add { get; }
        public IStateTransformer<string> TransformTestAsync { get; }
        public IStateTransformer<string> TransformTestSync { get; }

        public IStateProvider ProvideTestSync { get; }
        public IStateProvider ProvideTestAsync { get; }

        public IStateTransformer<(IntListVP.CMD_LIST CMD, CRUDTest? ITEM)> CRUDListCmds { get; }
        public IStateTransformer<(IntDictVP.CMD_DICT CMD, Guid? ID, CRUDTest? ITEM)> CRUDDictCmds { get; }

        public string Test { get; private set; } = string.Empty;

        public ServiceFixture() 
        {
            ObservableStateVoidProvider = this.CreateObservableStateProvider();
            ObservableIntState = this.CreateState(0);
            ObservableStateIntProvider = this.CreateObservableStateProvider(ObservableIntState);

            IntState = this.CreateState(-1);

            IntStateAsyncX = this.CreateState(10, s => new AsyncIntX(this, s));

            CRUDListState = this.CreateState<ServiceFixture, IEnumerable<CRUDTest>, List<CRUDTest>>([]);
            CRUDListCmds = new IntListVP(this, CRUDListState);

            CRUDDictState = this.CreateState<ServiceFixture, IEnumerable<KeyValuePair<Guid, CRUDTest>>, Dictionary<Guid, CRUDTest>>([]);
            CRUDDictCmds = new IntDictVP(this, CRUDDictState);

            Increment = new IncremementVP(this, IntState);
            Add = new AddVP(this, IntState);

            TransformTestSync = new TransformTestSyncST(this);
            TransformTestAsync = new TransformTestAsyncST(this);

            ProvideTestSync = new ProvideTestSyncSP(this);
            ProvideTestAsync = new ProvideTestAsyncSP(this);
        }

        public void ClearTest()
        {
            Test = string.Empty;
        }
    }
}
