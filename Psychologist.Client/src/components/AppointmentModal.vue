<template>
	<vue-final-modal class="modal-container"
									 content-class="modal-content"
									 content-transition="vfm-fade"
									 overlay-transition="vfm-fade">
		<div class="modal--header">
			<p class="modal-title">
				<span>Расписание на {{ date.toFormat('dd.MM.yyyy') }}</span>
			</p>
			<button class="modal-close" @click="emit('cancel')">
				<svg focusable="false" width="2em" height="2em" viewBox="0 0 24 24">
					<use href="#icon-x" />
				</svg>
			</button>
		</div>
		<div class="modal--content p-1">
			<div v-if="freeIntervals.length > 0">
				<div class="input-group mb-3">
					<span class="input-group-text">Время</span>
					<select class="form-select" v-model="selectedInterval" required>
						<option v-for="time in freeIntervals" :value="time">
							{{ time.start.toFormat('HH:mm') }} - {{ time.end.toFormat('HH:mm') }}
						</option>
					</select>
				</div>
				<div class="input-group mb-3">
					<span class="input-group-text">Тип консультации</span>
					<select class="form-select" v-model="selectedType" required>
						<option v-for="type in consultationTypes" :value="type.name">{{ type.title }}</option>
					</select>
				</div>
				<div class="form-check mb-2">
					<label class="form-check-label">
						<input class="form-check-input" type="checkbox" v-model="primary">
						Первичное обследование
					</label>
				</div>
			</div>
			<p v-else class="m-0">Нет доступного времени для записи</p>
		</div>
		<div class="modal--action">
			<button @click="submit" :disabled="freeIntervals.length === 0">Записаться</button>
		</div>
	</vue-final-modal>
</template>

<script setup>
import { ref, reactive, computed, watch } from "vue";
import { VueFinalModal } from 'vue-final-modal'
import { DateTime } from "luxon";
import { callGet } from "@/services/api.js";
import { dateTimeOptions } from "@/utils/timeUtils.js";
import { getConsultationTypesForSpecialist, visitorTypes } from "@/utils/commonUtils.js";

const emit = defineEmits([ 'submit', 'cancel' ])
const props = defineProps({
	date: DateTime,
	specialist: Object,
})

const consultationTypes = computed(() => getConsultationTypesForSpecialist(props.specialist.type))

const freeIntervals = ref([]);
const selectedInterval = ref(null);
const selectedType = ref(null);
const primary = ref(true);

const getFormattedData = () => ({
	date: props.date.toISODate(),
	time: selectedInterval.value?.start?.toFormat('HH:mm'),
	specialistId: props.specialist?.id,
	type: selectedType.value,
	primary: primary.value,
});

const submit = () => emit('submit', getFormattedData());

watch(props, async () => {
	let rawScheduleDay = await callGet(`/api/schedule/${ props.specialist?.id }/${ props.date.toISODate() }/appointment`);
	freeIntervals.value = rawScheduleDay.freeIntervals.map(parseFreeInterval);
}, { immediate: true })

const parseFreeInterval = s => ({
	start: DateTime.fromISO(s.start, dateTimeOptions),
	end: DateTime.fromISO(s.end, dateTimeOptions),
});


</script>