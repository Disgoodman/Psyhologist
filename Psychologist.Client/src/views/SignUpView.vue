<template>
	<form class="mx-auto card p-4 w-100 p-2 shadow-sm" @submit.prevent="register">
		<h1 class="h3 mb-3 text-center">Регистрация</h1>

		<label for="email" class="text-start ms-0">Email</label>
		<input type="email" v-model="email"
					 class="form-control mb-3" autocomplete="username" required autofocus>

		<label for="password" class="text-start">Пароль</label>
		<div class="input-group">
			<input type="password" v-model="password"
						 class="form-control rounded" autocomplete="current-password" required>
			<button type="button" class="d-none" v-toggle-password></button>
		</div>

		<button class="btn btn-lg btn-primary w-100 mt-3" type="submit">Зарегистрироваться</button>

	</form>
</template>

<script setup>
import { ref } from "vue";
import store from "@/store";
import router from "@/router";
import vTogglePassword from "@/utils/toggle-password-input";
import { postRaw } from "@/services/fetch.js";

const email = ref('a@a.a');
const password = ref('Aa123.');

async function register() {
	let registerResponse = await postRaw('/api/register', {
		email: email.value,
		password: password.value
	});

	await store.dispatch('login', { email: email.value, password: password.value });

	await router.push("/");
}

</script>

<style scoped>
form {
	max-width: 330px;
	margin-top: 17%;
}
</style>