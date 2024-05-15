<template>
  <div>
    <div class="mx-3 mt-3 d-flex align-items-center">
      <p class="m-0 me-3">Специалист</p>
      <select class="form-select" v-model="specialist">
        <option v-for="s in specialists" :value="s">{{ getFullName(s) }} ({{ s.type }})</option>
      </select>
    </div>

    <div v-if="specialist" class="mx-3 mt-2">
      <p class="m-0">Первичная консультация: {{ specialist.primaryVisitPrice }} руб.</p>
      <p class="m-0">Повторная консультация: {{ specialist.secondaryVisitPrice }} руб.</p>
    </div>

    <div v-show="scheduleLoaded" class="calendar mt-3" @clickDay="clickDay"></div>
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
import { callDelete, callGet, callPost, callPut } from "@/services/api.js";
import { useRouter } from "vue-router";
import { useModal } from "vue-final-modal";
import { getFullName } from "@/utils/commonUtils.js";
import { parseSchedule } from "@/store/modules/schedule.js";
import AppointmentModal from "@/components/AppointmentModal.vue";

const store = useStore();
const router = useRouter();

const specialist = ref(null);
watch(specialist, async s => {
  let data = await callGet(`/api/schedule/${s.id}`);
  data = data.map(parseSchedule);
  console.debug('schedule: ', data);
  schedule.value = data;
});

const year = ref(new Date().getFullYear());

const dateTypes = {
  'workday': { color: 'var(--event-workday-color)', name: 'Рабочий день', tippyTheme: 'event-workday' }
}

const schedule = ref(null);
const scheduleLoaded = computed(() => !!schedule.value);
const specialists = computed(() => store.state.common.specialists ?? []);

const calendarDataSource = computed(() => {
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

async function clickDay(event) {
	console.log(event);
  let startDate = DateTime.fromJSDate(event.date);

  if (event.events.length !== 0) {
      select(startDate);
  }
}

let calendar;
let tooltip = null;

onMounted(async () => {
  calendar = new Calendar('.calendar', {
    language: 'ru',
    enableRangeSelection: false,
    style: "background",
    startYear: year.value,
    dataSource: calendarDataSource.value,
    minDate: DateTime.now().plus({ days: -1 }).toJSDate(),
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

  if (!store.getters.specialistsLoaded)
    await store.dispatch('loadSpecialists');
});

function select(date) {
  appointmentModalVfmModalAttrs.date = date;
  appointmentModalVfmModalAttrs.specialist = specialist.value;
  appointmentModalVfmModal.open();
}

const appointmentModalVfmModal = useModal({
  component: AppointmentModal,
  attrs: {
    async onSubmit(data) {
      await callPost(`/api/appointment`, data);
      await appointmentModalVfmModal.close();
    },
    onCancel() {
      appointmentModalVfmModal.close();
    }
  },
})
const appointmentModalVfmModalAttrs = appointmentModalVfmModal.options.attrs;

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
