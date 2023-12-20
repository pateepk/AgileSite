import Vue from "vue"
import revux from 'revux'

import App from "./App.vue";

Vue.use(revux)

const vm: Vue = new Vue({
    el: "#app",
    template: "<app/>",
    components: { App }
});
