using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Blazor.ECharts
{
    // This class provides an example of how JavaScript functionality can be wrapped
    // in a .NET class for easy consumption. The associated JavaScript module is
    // loaded on demand when first needed.
    //
    // This class can be registered as scoped DI service and then injected into Blazor
    // components for use.

    public class JsInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;
        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        public JsInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/Blazor.ECharts/core.js").AsTask());
        }

        public async ValueTask<string> Prompt(string message)
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<string>("showPrompt", message);
        }

        /// <summary>
        /// ��ʼ��Echarts
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="id">ECharts����ID</param>
        /// <returns></returns>
        public async ValueTask<IJSObjectReference> InitChart(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts�ؼ�id����Ϊ��");
            var module = await moduleTask.Value;
            return await module.InvokeAsync<IJSObjectReference>("echartsFunctions.initChart", id);
        }

        /// <summary>
        /// ����Echarts����
        /// </summary>
        /// <param name="jsRuntime"></param>
        /// <param name="id">ECharts����ID</param>
        /// <param name="option">����</param>
        /// <param name="notMerge">��ѡ���Ƿ񲻸�֮ǰ���õ� option ���кϲ���Ĭ��Ϊ false�����ϲ���</param>
        /// <returns></returns>
        public async Task SetupChart(string id, object option)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts�ؼ�id����Ϊ��");
            if (option == null) throw new ArgumentNullException(nameof(option), "echarts��������Ϊ��");
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("echartsFunctions.setupChart", id, JsonSerializer.Serialize(option, jsonSerializerOptions));
        }

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}
