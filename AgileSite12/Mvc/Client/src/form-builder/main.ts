// Override CustomEvent polyfill before stencil's implementation, which causes errors in IE11.
// This solution is temporary until we upgrade stencil to newer version,
// where the polyfill implementation has already been fixed.
import "@/builder/custom-event";
import "@/builder/mouse-event";

// Load stencil components
import "../../dist/components";
import "../builder/assets/styles/form-builder.less";
import "../builder/assets/styles/generated/icon-font-face.less";

import { initContainer, resolveService } from "@/builder/container";
import { formBuilderContainer } from "@/form-builder/container-config";

import { FormBuilder } from "./FormBuilder";
import { FormBuilderOptions } from "./types";

initContainer(formBuilderContainer);

window.kentico = window.kentico || {};
window.kentico.formBuilder = {
  init: async (options: FormBuilderOptions) => {
    const formBuilder = resolveService<FormBuilder>(FormBuilder);
    await formBuilder.initializeFormBuilder(options);
  },
};
