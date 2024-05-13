//import  "bootstrap/dist/css/bootstrap-reboot.min.css"
import  "bootstrap/dist/css/bootstrap.min.css"
import "izitoast/dist/css/iziToast.min.css"
import "@/utils/toggle-password-input/style.css"
import 'vue-final-modal/style.css'

import { createApp } from 'vue'
import App from './App.vue'
import store from '@/store'
import router from '@/router'
import { createVfm } from 'vue-final-modal'

const vfm = createVfm();

createApp(App)
    .use(store)
    .use(router)
    .use(vfm)
    .mount('#app');
