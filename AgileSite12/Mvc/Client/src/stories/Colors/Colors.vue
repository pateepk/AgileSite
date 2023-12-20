<template>
  <div>
    <Palette v-for="palette in palettes" :key="palette.title" :title="palette.title" :colors="palette.colors" />
  </div>
</template>

<script>
import Palette from "./Palette.vue";

export default {
  components: {
    Palette
  },
  data() {
    return {
      palettes: [
        {
          title: "Gray",
          colors: [
            { name: "50", hex: "#262524", rgb: "38, 37, 36" },
            { name: "70", hex: "#403e3d", rgb: "38, 37, 36" },
            { name: "80", hex: "#504e4c", rgb: "38, 37, 36" },
            { name: "90", hex: "#696663", rgb: "38, 37, 36" },
            { name: "100", hex: "#a3a2a2", rgb: "38, 37, 36" },
            { name: "130", hex: "#bdbbbb", rgb: "38, 37, 36" },
            { name: "140", hex: "#d6d9d6", rgb: "38, 37, 36" },
            { name: "150", hex: "#e5e5e5", rgb: "38, 37, 36" },
            { name: "155", hex: "#eeeeee", rgb: "38, 37, 36" },
            { name: "160", hex: "#f7f7f7", rgb: "38, 37, 36" }
          ]
        },
        {
          title: "Green",
          colors: [
            { name: "70", hex: "#355e00", rgb: "38, 37, 36" },
            { name: "100", hex: "#497d04", rgb: "38, 37, 36" },
            { name: "120", hex: "#95be5f", rgb: "38, 37, 36" },
            { name: "130", hex: "#d7ebbf", rgb: "38, 37, 36" },
          ]
        },
        {
          title: "Blue",
          colors: [
            { name: "70", hex: "#0f6194", rgb: "38, 37, 36" },
            { name: "100", hex: "#1175ae", rgb: "38, 37, 36" },
            { name: "120", hex: "#73bae6", rgb: "38, 37, 36" },
            { name: "130", hex: "#d0e8ed", rgb: "38, 37, 36" },
          ]
        },
        {
          title: "Red",
          colors: [
            { name: "70", hex: "#b12628", rgb: "38, 37, 36" },
            { name: "100", hex: "#e14344", rgb: "38, 37, 36" },
            { name: "130", hex: "#ffdddd", rgb: "38, 37, 36" },
          ]
        },
        {
          title: "Orange",
          colors: [
            { name: "70", hex: "#a15700", rgb: "38, 37, 36" },
            { name: "80", hex: "#c98209", rgb: "38, 37, 36" },
            { name: "100", hex: "#f4eac9", rgb: "38, 37, 36" },
            { name: "130", hex: "#e5e0cb", rgb: "38, 37, 36" },
          ]
        },
        {
          title: "Purple",
          colors: [
            { name: "100", hex: "#784B84", rgb: "38, 37, 36" },
          ]
        },
        {
          title: "Apps",
          colors: [
            { name: "Facebook", hex: "#3b5998", rgb: "38, 37, 36" },
            { name: "Twitter", hex: "#55acee", rgb: "38, 37, 36" },
            { name: "LinkedIn", hex: "#007bb6", rgb: "38, 37, 36" },
            { name: "SharePoint", hex: "#1372b8", rgb: "38, 37, 36" },
          ]
        },
      ]
    };
  }
};
</script>

<style>
</style>
