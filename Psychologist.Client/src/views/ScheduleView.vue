<template>
  <div>
    <div class="btn-group mx-3 mt-3 d-flex" role="group">
      <input type="radio" class="btn-check" id="scheduleViewMode" v-model="editMode" :value="false">
      <label class="btn btn-outline-primary" for="scheduleViewMode">Режим просмотра</label>

      <input type="radio" class="btn-check" id="scheduleEditMode" v-model="editMode" :value="true">
      <label class="btn btn-outline-primary" for="scheduleEditMode">Режим редактирования</label>
    </div>

    <p v-show="editMode" class="mx-3 mt-2 p-0 text-secondary lh-sm">
      Для редактирования расписания нажмите на дату или выделите диапазон дат.
    </p>

    <div class="calendar mt-3" @selectRange="selectRange"></div>
  </div>
</template>

<script setup>
import { useStore } from "vuex";
import { computed, onMounted, reactive, ref, watch } from "vue";

import Calendar from 'js-year-calendar';
import 'js-year-calendar/locales/js-year-calendar.ru';
import 'js-year-calendar/dist/js-year-calendar.css';
import { DateTime } from "luxon";
import tippy from "tippy.js";
import 'tippy.js/dist/tippy.css';
import { callDelete, callPost, callPut } from "@/services/api.js";
import { useRouter } from "vue-router";
import { useModal } from "vue-final-modal";
import AddScheduleModal from "@/components/AddScheduleModal.vue";
import RemoveScheduleModal from "@/components/RemoveScheduleModal.vue";
import AddScheduleDayModal from "@/components/AddScheduleDayModal.vue";
import EditScheduleDayModal from "@/components/EditScheduleDayModal.vue";

const store = useStore();
const router = useRouter();

const year = ref(new Date().getFullYear());
const editMode = ref(false);

const dateTypes = {
  'workday': { color: 'var(--event-workday-color)', name: 'Рабочий день', tippyTheme: 'event-workday' }
}

const schedule = computed(() => store.state.common.schedule);
const scheduleLoaded = computed(() => store.getters.scheduleLoaded);

const calendarDataSource = computed(() => {
  console.log('scheduleData')
  let data = schedule.value ?? [];
  return data.map(e => ({
    startDate: e.date.toJSDate(),
    endDate: e.date.toJSDate(),
    type: 'workday',
    color: 'var(--event-workday-color)',
    data: e
  }));
});

watch(calendarDataSource, v => calendar.setDataSource(v))

async function selectRange(event) {
  let startDate = DateTime.fromJSDate(event.startDate);
  let endDate = DateTime.fromJSDate(event.endDate);

  if (editMode.value === true) {
    return await selectRangeInEditMode(event, startDate, endDate)
  } else {
    if (event.events.length !== 0) {
      await router.push({ name: 'scheduleDay', params: { date: startDate.toISODate() } })
    }
  }
}

async function selectRangeInEditMode(event, startDate, endDate) {
  console.log(startDate, endDate)
  if (event.events.length === 0) {
    if (startDate.equals(endDate)) {
      addScheduleDay(startDate);
    } else {
      addSchedule({ startDate, endDate });
    }
  } else {
    if (startDate.equals(endDate)) {
      editScheduleDay(startDate);
    } else {
      removeSchedule({ startDate, endDate });
    }
  }
}

let calendar;
let tooltip = null;

onMounted(async () => {
  calendar = new Calendar('.calendar', {
    language: 'ru',
    enableRangeSelection: true,
    style: "background",
    //style: "border",
    //selectRange: selectRange,
    startYear: year.value,
    dataSource: calendarDataSource.value,
    yearChanged(e) {
      year.value = e.currentYear;
    },
    mouseOnDay(e) {
      if (e.events.length === 0) return;
      let event = e.events[0];

      let content = DateTime.fromJSDate(event.startDate).toFormat('dd.MM.yyyy');
      content += '<br>' + dateTypes[event.type]?.name;
      if (event.data.breakTime)
        content += `<br>${event.data.startTime.toFormat('HH:mm')} - ${event.data.breakTime.toFormat('HH:mm')}, ${event.data.breakTime.plus({ hour: 1 }).toFormat('HH:mm')} - ${event.data.endTime.toFormat('HH:mm')}`;
      else
        content += `<br>${event.data.startTime.toFormat('HH:mm')} - ${event.data.endTime.toFormat('HH:mm')}`;

      if (tooltip != null) {
        tooltip.destroy();
        tooltip = null;
      }
      tooltip = tippy(e.element, {
        placement: 'bottom',
        content: content,
        allowHTML: true,
        theme: dateTypes[event.type]?.tippyTheme,
        animation: false,
        arrow: true
      });
      tooltip.show();
    },
    mouseOutDay: function () {
      if (tooltip !== null) {
        tooltip.destroy();
        tooltip = null;
      }
    }
  });

  await store.dispatch('loadSchedule');
});

function addSchedule({ startDate, endDate }) {
  addScheduleVfmModalAttrs.startDate = startDate;
  addScheduleVfmModalAttrs.endDate = endDate;
  addScheduleVfmModal.open();
}

const addScheduleVfmModal = useModal({
  component: AddScheduleModal,
  attrs: {
    async onSubmit(data) {
      let days = await callPost(`/api/schedule/range`, data);
      store.commit('addSchedule', days);
      await addScheduleVfmModal.close();
    },
    onCancel() {
      addScheduleVfmModal.close();
    }
  },
})
const addScheduleVfmModalAttrs = addScheduleVfmModal.options.attrs;

function addScheduleDay(date) {
  console.log(date)
  addScheduleDayVfmModalAttrs.date = date;
  addScheduleDayVfmModal.open();
}

const addScheduleDayVfmModal = useModal({
  component: AddScheduleDayModal,
  attrs: {
    async onSubmit(data) {
      let day = await callPost('/api/schedule', data);
      store.commit('addSchedule', day);
      await addScheduleDayVfmModal.close();
    },
    onCancel() {
      addScheduleDayVfmModal.close();
    }
  },
})
const addScheduleDayVfmModalAttrs = addScheduleDayVfmModal.options.attrs;


function editScheduleDay(date) {
  console.log(schedule.value, date, schedule.value.find(s => +s.date === +date));
  editScheduleDayVfmModalAttrs.date = date;
  editScheduleDayVfmModalAttrs.schedule = schedule.value.find(s => +s.date === +date); // equals not work
  editScheduleDayVfmModal.open();
}

const editScheduleDayVfmModal = useModal({
  component: EditScheduleDayModal,
  attrs: {
    async onSubmit(data) {
      console.log(data);
      let { date, ...schedule } = data;
      let day = await callPut('/api/schedule/' + date, schedule);
      store.commit('updateSchedule', day);
      await editScheduleDayVfmModal.close();
    },
    async onRemove(data) {
      await callDelete('/api/schedule/' + data.date)
      store.commit('deleteSchedule', data.date);
      await editScheduleDayVfmModal.close();
    },
    onCancel() {
      editScheduleDayVfmModal.close();
    }
  },
})
const editScheduleDayVfmModalAttrs = editScheduleDayVfmModal.options.attrs;


function removeSchedule({ startDate, endDate }) {
  removeScheduleVfmModalAttrs.startDate = startDate;
  removeScheduleVfmModalAttrs.endDate = endDate;
  removeScheduleVfmModal.open();
}

const removeScheduleVfmModal = useModal({
  component: RemoveScheduleModal,
  attrs: {
    async onSubmit(data) {
      await callDelete('/api/schedule/' + data.startDate + '/' + data.endDate)
      store.commit('deleteScheduleRange', data);
      await removeScheduleVfmModal.close();
    },
    onCancel() {
      removeScheduleVfmModal.close();
    }
  },
})
const removeScheduleVfmModalAttrs = removeScheduleVfmModal.options.attrs;

</script>

<style lang="scss">

:root {
  --event-workday-color: rgba(50, 50, 220, 0.3);
  --event-holiday-color: rgba(220, 50, 50, 0.4);
  --event-pre-holiday-color: rgba(245, 147, 111, 0.4);
  --event-weekend-color: rgba(220, 50, 50, 0.4);
}

/* Disable render animation */
.calendar .months-container {
  transition: none !important;
  opacity: 1 !important;
}

@media (max-width: 767px) {
  .calendar .calendar-header .year-neighbor2 {
    display: table-cell;
  }
  .calendar .calendar-header .year-neighbor {
    display: none;
  }
}

@media (max-width: 991px) {
  .calendar .calendar-header .year-neighbor2 {
    display: none;
  }
  .calendar .calendar-header .year-neighbor {
    display: table-cell;
  }
}

@each $theme, $color in ("event-workday", rgba(200, 200, 255, 0.8)), {
  .tippy-box[data-theme~=#{$theme}] {
    background-color: $color;
    color: black;
    text-align: center;
  }

  @each $side in (left, top, right, bottom) {
    .tippy-box[data-theme~=#{$theme}][data-placement^=#{$side}] > .tippy-arrow::before {
      border-#{$side}-color: $color;
    }
  }
}
</style>
