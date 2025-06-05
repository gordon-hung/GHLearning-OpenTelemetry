import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import elementPlus from 'element-plus'
import 'element-plus/dist/index.css'

// OpenTelemetry 相關導入
import { ConsoleSpanExporter,SimpleSpanProcessor, WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http';
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { ZoneContextManager } from '@opentelemetry/context-zone';
import { B3Propagator } from '@opentelemetry/propagator-b3';
import { DocumentLoadInstrumentation } from '@opentelemetry/instrumentation-document-load';
//import { getWebAutoInstrumentations } from '@opentelemetry/auto-instrumentations-web';
import { resourceFromAttributes } from '@opentelemetry/resources';

// 設置資源屬性
const resource = resourceFromAttributes({
     "service.name": 'opentelemetry-sample-web',
     "service.version": '1.0.0',
     "telemetry.sdk.language": 'javascript',
     "telemetry.sdk.name": 'opentelemetry',
});

// 初始化 OpenTelemetry Tracer
const provider = new WebTracerProvider({
  // 設置服務名稱
  serviceName: 'opentelemetry-sample-web',
  resource:resource,
  // 使用 SimpleSpanProcessor 並將其加入提供者配置
  spanProcessors: [
    new SimpleSpanProcessor(new OTLPTraceExporter({ 
      url: 'http://localhost:4318/v1/traces' })),
    new SimpleSpanProcessor(new ConsoleSpanExporter())
  ]
});

// 註冊提供者
provider.register({
  // Changing default contextManager to use ZoneContextManager - supports asynchronous operations - optional
  contextManager: new ZoneContextManager(),
  propagator: new B3Propagator(),
});

// Registering instrumentations
registerInstrumentations({
  instrumentations: [
    //getWebAutoInstrumentations({
          // load custom configuration for xml-http-request instrumentation
          //'@opentelemetry/instrumentation-xml-http-request': {
           // clearTimingResources: true,
          //},
       // }),
    new DocumentLoadInstrumentation()
  ],
});

const app = createApp(App)
app.use(elementPlus)
app.use(router)
app.mount('#app')