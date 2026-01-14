using System.Management;

namespace GoodiesControl.Services
{
    internal class ChargeLimitService
    {
        public async Task SetLimitAsync(int percentage)
        {
            var clamped = Math.Clamp(percentage, 50, 100);
            var buffer = BuildRequest(clamped);

            await Task.Run(() =>
            {
                var scope = new ManagementScope(@"\\.\ROOT\WMI");
                scope.Connect();
                using var searcher = new ManagementObjectSearcher(scope, new SelectQuery("SELECT * FROM OemWMIMethod"));
                foreach (ManagementObject obj in searcher.Get())
                {
                    obj.InvokeMethod("OemWMIfun", new object[] { buffer });
                    return;
                }
                throw new InvalidOperationException("未找到 OemWMIMethod WMI 类，无法设置充电限制。");
            });
        }

        private static byte[] BuildRequest(int percent)
        {
            var request = new byte[64];
            request[0] = 0x03; // MFID
            request[1] = 0x15; // SFID = SBCM
            request[2] = 0x01; // \SBCM.CHMD
            request[3] = 0x18; // \SBCM.DELY
            request[4] = (byte)(percent - 5); // start threshold
            request[5] = (byte)percent;       // stop threshold
            return request;
        }
    }
}
