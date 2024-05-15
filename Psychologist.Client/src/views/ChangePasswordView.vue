<template>
  <form class="mx-auto card p-4 w-100 p-2 shadow-sm" @submit.prevent="changePassword">
    <h1 class="h3 mb-3 text-center">Изменение пароля</h1>

    <label for="password" class="text-start">Старый пароль</label>
    <div class="input-group">
      <input type="password" v-model="oldPassword"
             class="form-control rounded" autocomplete="current-password" required>
      <button type="button" class="d-none" v-toggle-password></button>
    </div>
    
    <label for="password" class="text-start">Новый пароль</label>
    <div class="input-group">
      <input type="password" v-model="newPassword"
             class="form-control rounded" autocomplete="new-password" required>
      <button type="button" class="d-none" v-toggle-password></button>
    </div>

    <button class="btn btn-lg btn-primary w-100 mt-3" type="submit">Изменить</button>

  </form>
</template>

<script setup>
import { ref } from "vue";
import router from "@/router";
import vTogglePassword from "@/utils/toggle-password-input";
import { RequestError } from "@/exceptions.js";
import iziToast from "izitoast";
import { callPost } from "@/services/api.js";

const oldPassword = ref('');
const newPassword = ref('');

async function changePassword() {
	await callPost('/api/change-password', { oldPassword: oldPassword.value, newPassword: newPassword.value });	
  
	iziToast.success({
    title: 'Пароль изменён',
    layout: 1,
    timeout: 3000,
  });
}

</script>

<style scoped>

form {
  max-width: 330px;
  margin-top: 5%;
}

</style>