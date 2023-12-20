import Vue from "vue"

import { LocalizationService } from "@/builder/declarations";

declare module "vue/types/vue" {
  // Declare augmentation for Vue
  interface Vue {
    $_localizationService: LocalizationService;
  }
}
