[assembly: WebActivator.PreApplicationStartMethod(typeof(NetBash.Membership.Sample.App_Start.NetBashStart), "Start")] 
namespace NetBash.Membership.Sample.App_Start {
    public static class NetBashStart {
        public static void Start() {
			NetBash.Init();
			
			//TODO: replace with your own auth code
			//NetBash.Settings.Authorize = (request) =>
			//	{
			//		return request.IsLocal;
			//	};
        }
    }
}