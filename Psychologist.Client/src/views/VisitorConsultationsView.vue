<template>
	<div>
		<div class="container-md mt-3">
			<div class="list-group">
				<div v-for="consultation in consultations"
						 class="list-group-item list-group-item-action active1">
					<p class="mb-1 text-center">
						{{ consultation.scheduleDate.toFormat('dd.MM.yyyy') }} {{ consultation.time.toFormat('HH:mm') }}
					</p>

					<p class="mb-1 text-truncate">Специалист: {{ getSpecialistLabel(consultation.specialist) }}</p>
					<p class="mb-1 text-truncate">Тема: {{ consultation.topic }}</p>
					<p class="mb-1 text-truncate">Тип: {{ getConsultationTypeTitleByName(consultation.type) }}</p>
					<p class="mb-1">Характер консультации: {{ consultation.primary ? 'первичная' : 'повторная' }}</p>

					<template v-if="consultation.type === 'individualConsultation'">
						<p class="mb-1">Код обращения: {{ consultation.requestCode }}</p>
						<p class="mb-1">Заметки: {{ consultation.notes }}</p>
					</template>
					<template v-else-if="consultation.type === 'individualWork'">
						<p class="mb-1">Цель: {{ consultation.purpose }}</p>
					</template>
					<template v-else-if="consultation.type === 'diagnosticWork'">
						<p class="mb-1">Вид: {{ consultation.primary ? 'первичное' : 'повторное' }} обследование</p>
						<p class="mb-1">Код обращения: {{ consultation.requestCode }}</p>
						<p class="mb-1">Тема обращения: {{ consultation.subject }}</p>
					</template>
					
					<button v-if="consultation.dateTime > now"
									@click="cancelConsultation(consultation)"
									class="btn btn-outline-secondary btn-sm">
						Отменить запись
					</button>
				</div>
			</div>
		</div>
	</div>
</template>

<script setup>
import { useStore } from "vuex";
import { onMounted, ref } from "vue";
import { DateTime } from "luxon";
import 'tippy.js/dist/tippy.css';
import { callGet, callPost } from "@/services/api.js";
import { parseConsultation } from "@/store/modules/common.js";
import { getConsultationTypeTitleByName, getSpecialistLabel } from "../utils/commonUtils.js";

const store = useStore();

const consultations = ref([]);

const now = DateTime.now();

onMounted(async () => {
	let c = await callGet(`/api/visitor-consultations`);
	consultations.value = c.map(parseConsultation);
});

async function cancelConsultation(consultation) {
	await callPost(`/api/cancel-consultation/${consultation.id}`);
	consultations.value = consultations.value.filter(c => c.id !== consultation.id);
}

</script>

<style lang="scss">

</style>
