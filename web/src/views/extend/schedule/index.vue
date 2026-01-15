<template>
  <div class="page-content-wrapper bg-white">
    <div class="page-content-wrapper-center schedule-container">
      <FullCalendar class="h-full" ref="fullCalendarRef" :options="calendarOptions" />
    </div>
  </div>
  <Form @register="registerForm" @refresh="initData" />
</template>
<script lang="ts" setup>
  import { reactive, ref } from 'vue';
  import FullCalendar from '@fullcalendar/vue3';
  import { CalendarOptions } from '@fullcalendar/core';
  import dayGridPlugin from '@fullcalendar/daygrid';
  import interactionPlugin from '@fullcalendar/interaction';
  import timeGridPlugin from '@fullcalendar/timegrid';
  import { useModal } from '/@/components/Modal';
  import dayjs from 'dayjs';
  import Form from './Form.vue';
  import { getSchedule } from '/@/api/extend/schedule';

  defineOptions({ name: 'extend-schedule' });

  const fullCalendarRef = ref();
  const state = reactive({
    startTime: '',
    endTime: '',
  });
  const calendarOptions = reactive<CalendarOptions>({
    plugins: [dayGridPlugin, interactionPlugin, timeGridPlugin],
    initialView: 'dayGridMonth',
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'dayGridMonth,timeGridWeek,timeGridDay',
    },
    events: [], //数据
    eventColor: '#378006', //事件背景颜色
    eventClick: handleEventsClick,
    dateClick: handleDateClick,
    editable: false, // 是否可以进行（拖动、缩放）修改
    eventStartEditable: false, // Event日程开始时间可以改变，默认true，如果是false其实就是指日程块不能随意拖动，只能上下拉伸改变他的endTime
    eventDurationEditable: false, // Event日程的开始结束时间距离是否可以改变，默认true，如果是false则表示开始结束时间范围不能拉伸，只能拖拽
    selectable: false, // 是否可以选中日历格
    selectMirror: false,
    selectMinDistance: 0, // 选中日历格的最小距离
    dayMaxEvents: false,
    weekends: true,
    navLinks: false, // 天链接
    slotEventOverlap: false,
    datesSet: datesRender,
    dayMaxEventRows: 2,
    locale: 'zh',
    aspectRatio: 1.65,
    buttonText: { today: '今日', month: '月', week: '周', day: '日' },
    allDayText: '全天',
    views: {
      //对应月视图
      dayGridMonth: {
        displayEventTime: false, //是否显示时间
        dayMaxEventRows: 4,
        moreLinkClick: 'popover',
      },
    },
  });
  const [registerForm, { openModal: openFormModal }] = useModal();

  function initData() {
    let query = {
      startTime: state.startTime,
      endTime: state.endTime,
    };
    getSchedule(query).then(res => {
      calendarOptions.events = res.data.list.map(o => ({
        id: o.id,
        title: o.content,
        start: dayjs(o.startTime).format('YYYY-MM-DD HH:mm:ss'),
        end: dayjs(o.endTime).format('YYYY-MM-DD HH:mm:ss'),
        color: o.colour,
        editable: false,
        allDay: false,
      }));
    });
  }
  function datesRender(calendar) {
    let view = calendar.view;
    state.startTime = dayjs(view.activeStart).format('YYYY-MM-DD HH:mm');
    state.endTime = dayjs(view.activeEnd).format('YYYY-MM-DD HH:mm');
    initData();
  }
  function handleDateClick(data) {
    let startTime = dayjs(data.date).format('YYYY-MM-DD HH:00');
    let clickTime = dayjs(data.date).format('YYYY-MM-DD');
    let currTime = dayjs().format('YYYY-MM-DD');
    if (clickTime < currTime) return;
    if (clickTime == currTime) {
      let thisDate = new Date();
      thisDate.setHours(thisDate.getHours() + 1);
      startTime = dayjs(thisDate).format('YYYY-MM-DD HH:00');
    }
    openFormModal(true, { startTime: new Date(startTime).getTime(), id: '' });
  }
  function handleEventsClick(data) {
    openFormModal(true, { id: data.event.id });
  }
</script>
<style lang="less">
  .schedule-container {
    padding: 0;
    height: 100%;

    .fc-toolbar.fc-header-toolbar {
      padding: 10px;
      margin-bottom: 0;
    }

    .fc-button-primary {
      background-color: @primary-color !important;
      border-color: @primary-color !important;
      height: 32px;
      line-height: 32px;
      padding: 0 0.65em;
      font-size: 12px;
    }

    .fc-button-primary:not(:disabled):active,
    .fc-button-primary:not(:disabled).fc-button-active {
      background-color: @primary-5 !important;
      border-color: @primary-5 !important;
    }

    .fc-button-primary:not(:disabled):focus {
      box-shadow: unset !important;
    }

    .fc-button .fc-icon {
      line-height: 16px;
    }

    .fc-view th {
      height: 40px;
      line-height: 40px;
      font-size: 12px;
      color: #909399;
      font-weight: normal;
      background: @app-content-background;
    }

    .fc .fc-popover {
      z-index: 999 !important;
    }

    .fc-popover {
      z-index: 999 !important;
    }

    .fc-center {
      color: @text-color-base;
    }

    .fc-view th,
    .fc-view td,
    .fc-view thead,
    .fc-view tbody,
    .fc-view .fc-divider,
    .fc-view .fc-row,
    .fc-view .fc-content,
    .fc-view .fc-popover,
    .fc-view .fc-list-view,
    .fc-view .fc-list-header td {
      border-color: #ebeef5;
      color: @text-color-base;

      a {
        color: @text-color-base;
      }
    }
  }

  html[data-theme='dark'] {
    .schedule-container {
      .fc-theme-standard .fc-scrollgrid {
        border: 1px solid #303030 !important;
      }

      .fc-theme-standard td,
      .fc-theme-standard th {
        border: 1px solid #303030 !important;
      }
    }
  }
</style>
