<template>
  <div>
    <div class="container-md mt-3">
      <router-link :to="store.getters.isSpecialist ? ({ name: 'specialistSchedule' }) : ({ name: 'schedule' })"
                   class="btn btn-outline-secondary btn-sm px-5">
        Назад
      </router-link>
    </div>
    
    <div class="container-md mt-3">
      <div class="list-group">
        <router-link v-for="interval in schedule?.consultations"
                     :class="{ disabled: interval.isBreak }"
                     :to="{ name: 'consultation', params: { specialistId: props.specialistId, date: props.date.toISODate(), time: interval.start.toFormat('HH:mm') } }"
                     class="list-group-item list-group-item-action active1">
          <p class="mb-1 text-center">{{ interval.start.toFormat('HH:mm') }} - {{ interval.end.toFormat('HH:mm') }}</p>
          <p v-if="interval.isBreak" class="mb-1 text-center">Перерыв</p>

          <template v-if="interval.consultation">
            <p class="mb-1">Посетитель: {{ getVisitorLabel(interval.consultation.visitor) }}</p>
            <p class="mb-1 text-truncate">Тема: {{ interval.consultation.topic }}</p>
            <p class="mb-1 text-truncate">Тип: {{ getConsultationTypeTitleByName(interval.consultation.type) }}</p>
            <p class="mb-1">Характер консультации: {{ interval.consultation.primary ? 'первичная' : 'повторная' }}</p>

            <template v-if="interval.consultation.type === 'individualConsultation'">
              <p class="mb-1">Код обращения: {{ interval.consultation.requestCode }}</p>
            </template>
            <template v-else-if="interval.consultation.type === 'individualWork'">
              <p class="mb-1">Цель: {{ interval.consultation.purpose }}</p>
            </template>
            <template v-else-if="interval.consultation.type === 'diagnosticWork'">
              <p class="mb-1">Вид: {{ interval.consultation.primary ? 'первичное' : 'повторное' }} обследование</p>
              <p class="mb-1">Код обращения: {{ interval.consultation.requestCode }}</p>
              <p class="mb-1">Тема обращения: {{ interval.consultation.subject }}</p>
            </template>
          </template>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
import { useStore } from "vuex";
import { onMounted, ref } from "vue";
import { DateTime } from "luxon";
import 'tippy.js/dist/tippy.css';
import { callGet } from "@/services/api.js";
import { parseConsultation } from "@/store/modules/common.js";
import { getVisitorLabel, getConsultationTypeTitleByName } from "../utils/commonUtils.js";

const store = useStore();

const props = defineProps({
  specialistId: Number,
  date: DateTime
})

const schedule = ref(null);

onMounted(async () => {
  let rawSchedule = await callGet(`/api/schedule/${props.specialistId}/${props.date.toISODate()}`);
  schedule.value = parseScheduleDay(rawSchedule);
});

const parseScheduleDay = day => ({
  ...day,
  consultations: day.consultations.map(c => ({
    ...c,
    start: DateTime.fromISO(c.start),
    end: DateTime.fromISO(c.end),
    consultation: c.consultation ? parseConsultation(c.consultation) : c.consultation
  }))
})

</script>

<style lang="scss">

</style>
