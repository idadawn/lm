<template>
  <div>
    <a-row name="basic" :label-col="{ span: 4 }" :wrapper-col="{ span: 16 }" autocomplete="off" class="sourceRow">
      <a-row class="sourceHeader">
        <a-col :span="24" style="height: 55px">
          <a-row>
            <a-col>
              <span class="icon-ym icon-ym-dataApplication" style="color: #317bb2; font-size: 40px"></span>
            </a-col>
            <a-col style="margin-left: 10px">
              <h3 style="font-size: 20px; margin: 0">数据库</h3>
              <p>请选择需要新增的数据库</p>
            </a-col>
          </a-row>
        </a-col>
      </a-row>
      <a-row class="sourceList">
        <h3>文件</h3>
        <div>
          <img
            src="../../../../assets/images/dataSource/excel1.png"
            style="padding: 10px 10px"
            @click="dbTypeHander('Excel')" />
          <img src="../../../../assets/images/dataSource/file-csv.png" @click="dbTypeHander('CSV')" />
        </div>
        <h3>关系型数据库</h3>
        <div>
          <img src="../../../../assets/images/dataSource/MySQL.png" @click="dbTypeHander('MySQL')" />
          <img
            src="../../../../assets/images/dataSource/Oracle1.png"
            style="padding: 22px 5px"
            @click="dbTypeHander('Oracle')" />
          <img src="../../../../assets/images/dataSource/SQLserver.png" @click="dbTypeHander('SQLServer')" />
          <img src="../../../../assets/images/dataSource/opstgresql.png" @click="dbTypeHander('PostgreSQL')" />
          <img
            src="../../../../assets/images/dataSource/greenplum.png"
            style="padding: 10px 5px"
            @click="dbTypeHander('Greenplum')" />
          <img src="../../../../assets/images/dataSource/SAPHANA.png" @click="dbTypeHander('SAPHANA')" />
          <img
            src="../../../../assets/images/dataSource/damengshujuku.png"
            style="padding: 15px 5px"
            @click="dbTypeHander('Damengshujuku')" />
        </div>
        <h3>非关系型数据库</h3>
        <div>
          <img src="../../../../assets/images/dataSource/icon_Solr.png" @click="dbTypeHander('Solr')" />
          <img src="../../../../assets/images/dataSource/Hive.png" @click="dbTypeHander('Hive')" />
          <img src="../../../../assets/images/dataSource/hbase.png" @click="dbTypeHander('Hbase')" />
          <img src="../../../../assets/images/dataSource/influxDB.png" @click="dbTypeHander('InfluxDB')" />
          <img src="../../../../assets/images/dataSource/ClickHouse.png" @click="dbTypeHander('ClickHouse')" />
        </div>
        <h3>http数据源</h3>
        <div>
          <img
            src="../../../../assets/images/dataSource/http1.png"
            style="padding: 12px 10px"
            @click="dbTypeHander('HTTP')" />
          <img src="../../../../assets/images/dataSource/MQTT.png" @click="dbTypeHander('MQTT')" />
        </div>
      </a-row>
    </a-row>
  </div>
</template>
<script lang="ts" setup>
  // import { reactive, ref, onMounted, watch } from 'vue';
  const emit = defineEmits(['getdbType']);

  function dbTypeHander(type) {
    emit('getdbType', type);
  }
</script>
<style lang="less" scoped>
  .sourceRow {
    display: flex;
    flex-flow: column wrap;
    .sourceHeader {
      padding: 12px 35px 12px 35px;
      background: #eceef5;
    }
    .sourceList {
      padding: 25px 35px 12px 35px;
      display: flex;
      flex-flow: column wrap;
      background: #fff;
      div {
        display: flex;
        margin-bottom: 20px;
        img {
          height: 60px;
          width: auto;
          padding: 5px;
          border: 1px solid #d6d5d5;
          margin-right: 20px;
          cursor: pointer;
        }
      }
    }
  }
</style>
