<template>
  <header>
    <nav class="py-2 bg-light border-bottom">
      <div class="container d-flex flex-wrap">
        <ul class="nav me-auto">
          <li class="nav-item">
            <router-link :to="{ name: 'home' }"
                         class="nav-link link-dark px-2"
                         :class="{active: router.currentRoute.value.name === 'home'}"
                         aria-current="page">
              Новости
            </router-link>
          </li>
          <li class="nav-item" v-if="isAdmin" :class="{active: router.currentRoute.value.name === 'schedule'}">
            <router-link :to="{ name: 'schedule' }" class="nav-link link-dark px-2">Расписание</router-link>
          </li>
          <li class="nav-item" v-if="isAdmin" :class="{active: router.currentRoute.value.name === 'specialists'}">
            <router-link :to="{ name: 'specialists' }" class="nav-link link-dark px-2">Специалисты</router-link>
          </li>
          <li class="nav-item" v-if="isAdmin" :class="{active: router.currentRoute.value.name === 'visitors'}">
            <router-link :to="{ name: 'visitors' }" class="nav-link link-dark px-2">Посетители</router-link>
          </li>

          <li class="nav-item" v-if="isVisitor" :class="{active: router.currentRoute.value.name === 'visitorConsultations'}">
            <router-link :to="{ name: 'visitorConsultations' }" class="nav-link link-dark px-2">Записи</router-link>
          </li>
          <li class="nav-item" v-if="isVisitor" :class="{active: router.currentRoute.value.name === 'appointment'}">
            <router-link :to="{ name: 'appointment' }" class="nav-link link-dark px-2">Запись</router-link>
          </li>
          
          <li class="nav-item" v-if="isSpecialist" :class="{active: router.currentRoute.value.name === 'specialistSchedule'}">
            <router-link :to="{ name: 'specialistSchedule' }" class="nav-link link-dark px-2">Записи</router-link>
          </li>
        </ul>
        <ul class="nav">
          <li class="nav-item" v-if="!isAuth" :class="{active: router.currentRoute.value.name === 'register'}">
            <router-link :to="{ name: 'register' }" class="nav-link link-dark px-2">Регистрация</router-link>
          </li>
          <li class="nav-item" v-if="!isAuth" :class="{active: router.currentRoute.value.name === 'login'}">
            <router-link :to="{ name: 'login' }" class="nav-link link-dark px-2">Вход</router-link>
          </li>

          <li class="nav-item" v-if="isAuth" :class="{active: router.currentRoute.value.name === 'changePassword'}">
            <router-link :to="{ name: 'changePassword' }" class="nav-link link-dark px-2">Изменить пароль</router-link>
          </li>
          <li class="nav-item" v-if="isAuth">
            <button @click="logout" class="nav-link link-dark px-2">Выход</button>
          </li>
        </ul>
      </div>
    </nav>
  </header>

  <main>
    <router-view/>
  </main>

  <ModalsContainer />
  <Icons />
</template>

<script setup>
import { useStore } from 'vuex'
import { useRouter } from 'vue-router'
import { computed, onErrorCaptured, watch } from "vue";
import iziToast from "izitoast";
import { RequestError } from "@/exceptions.js";
import { errorToText } from "@/utils/commonUtils.js";
import Icons from "@/components/Icons.vue";
import { ModalsContainer } from "vue-final-modal";

const store = useStore();
const router = useRouter();

const isAuth = computed(() => store.getters.isAuth);
const isAdmin = computed(() => store.getters.isAdmin);
const isSpecialist = computed(() => store.getters.isSpecialist);
const isVisitor = computed(() => store.getters.isVisitor);

onErrorCaptured((err, vm, info) => {
  if (err instanceof RequestError) {
    console.error(err);
    if (err.rfc7807) {
      iziToast.error({
        title: 'Ошибка запроса.',
        message: errorToText(err),
        layout: 2,
        timeout: 5000,
        class: "iziToast-api-error"
      });
    } else {
      iziToast.error({
        title: err.message ?? 'Неизвестная ошибка запроса.',
        message: 'Код: ' + err.status,
        timeout: 5000,
        class: "iziToast-api-error"
      });
    }
    return false;
  }
});

function logout() {
  store.dispatch('logout')
}

watch(isAuth, auth => {
  console.log('auth', auth)
  if (!auth) router.push('/login')
})

</script>

<style>

/* modal styles */

.modal-container {
  display: flex;
  justify-content: center;
  align-items: center;
}

.modal-content { /*relative p-4 rounded-lg bg-white dark:bg-gray-900*/
  position: relative;
  display: flex;
  flex-direction: column;
  max-height: 90%;
  max-width: 90vw;
  margin: 0 1rem;
  padding: 1rem;
  border: 1px solid #e2e8f0;
  border-radius: 0.25rem;
  background: #fff;
}

.modal--header {
  display: flex;
  align-items: center;
}
.modal-title {
  margin: 0 32px 0 0;
  font-size: 1.5rem;
  font-weight: 600;
  flex-grow: 1;
}
.modal-close {
  border: none;
  background-color: transparent;
  cursor: pointer;
  padding: 0;
  line-height: 0;
}

.modal--content {
  margin-top: 10px;
  overflow-y: auto;
}

.modal--content .error-message {
  margin: 0 !important;
}

.modal--action {
  display: flex;
  justify-content: center;
  align-items: center;
  flex-shrink: 0;
  padding: 8px 0 0;
}

.modal--action > button {
  flex-grow: 1;
  padding: 0.25rem 0.5rem;
  border-width: 1px;
  border-radius: 0.25rem;
  background-color: transparent;
  background-image: none;
  cursor: pointer;
}

.modal--action > button + button {
  margin-left: 12px;
}

.modal--action > button:not(:disabled):hover {
  background-color: rgba(7, 84, 197, 0.1)
}

</style>
