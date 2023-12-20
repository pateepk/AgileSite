<template>
  <ul>
    <li v-for="item in items">
      {{ item }}
    </li>
  </ul>
</template>

<script lang="ts">
import { Vue, Component, Prop } from "vue-property-decorator";

@Component
class List extends Vue {

    @Prop()
    items: string[];
}

export default List;
</script>
