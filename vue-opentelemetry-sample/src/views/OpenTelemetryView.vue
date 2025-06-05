<template>
  <div>
    <h1>OpenTelemetry Page</h1>
    
    <!-- 按鈕來重新發送 API 請求 -->
    <div>
      <el-button type="primary" @click="fetchData">重新載入資料</el-button>
    </div>

    <!-- 顯示加載中或錯誤訊息 -->
    <div v-if="loading">正在加載...</div>
    <div v-if="error" style="color: red;">出現錯誤：{{ error }}</div>

    <!-- 顯示請求的 Headers -->
    <div v-if="requestHeaders">
      <h2>請求 Headers：</h2>
      <el-table :data="requestHeadersTableData" style="width: 100%">
        <el-table-column label="Key" prop="key"></el-table-column>
        <el-table-column label="Value" prop="value"></el-table-column>
      </el-table>
    </div>

    <!-- 顯示回應的 Headers -->
    <div v-if="responseHeaders">
      <h2>回應 Headers：</h2>
      <el-table :data="responseHeadersTableData" style="width: 100%">
        <el-table-column label="Key" prop="key"></el-table-column>
        <el-table-column label="Value" prop="value"></el-table-column>
      </el-table>
    </div>

    <!-- 顯示從 API 取得的資料 -->
    <div v-if="data">
      <h2>API 回應資料：</h2>
      <el-table :data="tableData" style="width: 100%">
        <el-table-column label="日期" prop="date"></el-table-column>
        <el-table-column label="攝氏溫度" prop="temperatureC"></el-table-column>
        <el-table-column label="華氏溫度" prop="temperatureF"></el-table-column>
        <el-table-column label="天氣概況" prop="summary"></el-table-column>
      </el-table>
    </div>

  </div>
</template>

<script>
import axios from 'axios';
import { trace } from '@opentelemetry/api';
import { v4 as uuidv4 } from 'uuid';

export default {
  name: 'OpenTelemetryView',
  data() {
    return {
      data: null,            // 用來存放 API 返回的資料
      loading: true,         // 加載狀態
      error: null,           // 錯誤訊息
      requestHeaders: null,  // 用來存放請求的 Headers
      responseHeaders: null, // 用來存放回應的 Headers
      tableData: [],         // 用來顯示 API 回應資料的表格資料
      requestHeadersTableData: [], // 用來顯示請求 Headers 的表格資料
      responseHeadersTableData: [] // 用來顯示回應 Headers 的表格資料
    };
  },
  mounted() {
    // 在組件掛載後進行 API 請求
    this.fetchData();
  },
  methods: {   
    // 呼叫 API 的方法
    async fetchData() {
      this.loading=false;
      this.error=null;
      this.requestHeaders=null;
      this.responseHeaders=null;
      this.data=null;
      // 獲取 tracer 實例
      const tracer = trace.getTracer('opentelemetry-sample-web');
      // 使用 tracer 來創建 span  
      const span = tracer.startSpan('OpenTelemetryView.vue');

      const correlationId=uuidv4()
      span.setAttribute('X-Correlation-Id', correlationId);
      try {
        const config = {
          headers: {
            'X-Correlation-Id': correlationId, // 加入 X-Correlation-Id
            'traceparent':'00-' + span._spanContext.traceId + '-' + span._spanContext.spanId +'-'+ span._spanContext.traceFlags.toString().padStart(2, "0") ,
          }};

        // 發送請求並記錄請求的 headers
        const response = await axios.get('https://localhost:7266/api/weatherforecast', config);
        
        // span 停止
        span.end();

        // 如果請求成功，將返回的資料儲存在 data 中
        this.data = response.data;

        // 將請求的 headers 儲存在 requestHeaders 中
        this.requestHeaders = config.headers;

        // 將回應的 headers 儲存在 responseHeaders 中
        this.responseHeaders = response.headers;

        // 將 data 轉換為表格資料
        this.tableData = this.data.map(item => ({
          date: item.date,
          temperatureC: item.temperatureC,
          temperatureF: item.temperatureF,
          summary: item.summary
        }));

        // 將 requestHeaders 轉換為表格資料
        this.requestHeadersTableData = Object.entries(this.requestHeaders || {}).map(([key, value]) => ({ key, value }));

        // 將 responseHeaders 轉換為表格資料
        this.responseHeadersTableData = Object.entries(this.responseHeaders || {}).map(([key, value]) => ({ key, value }));

      } catch (error) {
        // 如果請求失敗，顯示錯誤訊息
        span.recordException(error);
        span.setStatus({ message: error.message });
        span.end();
        this.error = error.message;
      } finally {
        // 不管請求成功或失敗，都將 loading 設為 false
        this.loading = false;
      }
    }
  }
};
</script>