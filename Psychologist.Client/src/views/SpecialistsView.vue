<template>
  <div class="container-lg mt-3">

    <div v-if="!specialistsLoaded" class="d-flex justify-content-center">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Загрузка...</span>
      </div>
    </div>

    <div v-else-if="specialists.length === 0" class="alert alert-warning d-flex justify-content-center" role="alert">
      Специалистов нет
    </div>

    <div v-if="newSpecialistFormVisible === false" class="mb-3 d-flex">
      <button type="button" class="btn btn-outline-secondary flex-grow-1"
              @click="newSpecialistFormVisible = true">Добавить специалиста
      </button>
    </div>

    <div v-if="newSpecialistFormVisible" class="card mb-3">
      <div class="card-body">
        <div class="input-group mb-3">
          <span class="input-group-text">Имя</span>
          <input type="text" class="form-control" v-model="newSpecialist.firstName">
        </div>
        <div class="input-group mb-3">
          <span class="input-group-text">Фамилия</span>
          <input type="text" class="form-control" v-model="newSpecialist.lastName">
        </div>
        <div class="input-group mb-3">
          <span class="input-group-text">Отвество</span>
          <input type="text" class="form-control" v-model="newSpecialist.patronymic">
        </div>
        <div class="input-group mb-3">
          <span class="input-group-text">Специальность</span>
          <input type="text" class="form-control" v-model="newSpecialist.type">
        </div>
        <div class="input-group mb-3">
          <span class="input-group-text">Цена первичной/вторичной кносультации, руб</span>
          <input type="number" class="form-control" v-model="newSpecialist.primaryVisitPrice">
          <input type="number" class="form-control" v-model="newSpecialist.secondaryVisitPrice">
        </div>
        <div class="input-group mb-3">
          <span class="input-group-text">Email</span>
          <input type="email" class="form-control" v-model="newSpecialist.email">
        </div>
        <div class="input-group mb-3">
          <span class="input-group-text">Пароль</span>
          <input type="text" class="form-control" v-model="newSpecialist.password">
        </div>

        <div class="d-flex flex-row-reverse">
          <button type="button" class="btn btn-outline-secondary" @click="newSpecialistFormVisible = false">
            Отмена
          </button>
          <button type="button" class="btn btn-outline-secondary me-1" @click="addSpecialist">Добавить</button>
        </div>
      </div>
    </div>

    <form class="mb-3 d-flex" @submit.prevent="search">
      <input v-model="searchText" class="flex-grow-1 form-control" placeholder="ФИО / Специальность">
      <button type="submit" class="ms-1 btn btn-outline-secondary">Поиск</button>
    </form>

    <table class="table table-hover specialists-table">
      <thead>
      <tr>
        <th scope="col">Email</th>
        <th scope="col">ФИО</th>
        <th scope="col">Специальность</th>
        <th scope="col">Цена первичной/вторичной консультации, руб</th>
        <th scope="col">Действия</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="specialist in filteredSpecialists" @click="openSpecialistPage(specialist)">
        <td>{{ specialist.email }}</td>
        <th scope="row">{{ getFullName(specialist) }}</th>
        <td>{{ specialist.type }}</td>
        <td>{{ specialist.primaryVisitPrice }} / {{ specialist.secondaryVisitPrice }}</td>
        <td>
          <div class="d-flex">
            <button type="button" class="btn btn-outline-secondary btn-sm"
                    @click.stop="deleteSpecialist(specialist)">
              Удалить
            </button>
          </div>
        </td>
      </tr>
      </tbody>
    </table>

  </div>
</template>

<script setup>
import { reactive, ref, watch } from "vue";
import { useStore } from "vuex";
import { computed, onMounted } from "vue";
import { callDelete, callPost, callPut } from "@/services/api.js";
import { useRouter } from "vue-router";
import { getFullName } from "@/utils/commonUtils.js";

const store = useStore();
const router = useRouter();

const specialists = computed(() => store.state.common.specialists);
const filteredSpecialists = ref([])
const specialistsLoaded = computed(() => store.getters.specialistsLoaded);

watch(specialists, v => filteredSpecialists.value = v, { immediate: true })

const newSpecialist = reactive({
  email: '', password: '',
  firstName: '', lastName: '', patronymic: '', type: '',
  primaryVisitPrice: 0, secondaryVisitPrice: 0
});
const newSpecialistFormVisible = ref(false);
const searchText = ref('');

onMounted(async () => {
  await store.dispatch('loadSpecialists');
});

async function addSpecialist() {
  let specialist = await callPost(`/api/specialists`, newSpecialist);
  store.commit('addSpecialist', specialist);
  newSpecialist.title = newSpecialist.text = '';
  newSpecialistFormVisible.value = false;
}

async function deleteSpecialist(specialist) {
  await callDelete(`/api/specialists/${specialist.id}`);
  store.commit('deleteSpecialist', specialist.id);
}

function openSpecialistPage(specialist) {
  router.push({ name: 'specialist', params: { id: specialist.id } });
}

function search() {
  filteredSpecialists.value = specialists.value.filter(c =>
      (getFullName(c) + ' ' + c.type).toLowerCase().includes(searchText.value.toLowerCase()));
}


</script>

<style scoped>
h5 {
  white-space: pre-wrap;
}

.specialists-table > tbody > tr {
  cursor: pointer;
}
</style>