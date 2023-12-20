import { builderConfig, http } from "@/builder/api/client";
import { PageBuilderConfig } from "@/page-builder/PageBuilderConfig";

const changeTemplate = async (templateIdentifier: string, guid: string): Promise<any> => {
  const endpoint = (builderConfig as PageBuilderConfig).pageTemplate.changeTemplateEndpoint;

  const headers = { [builderConfig.constants.editingInstanceHeader]: guid };
  return http.post(endpoint, templateIdentifier, { headers });
};

export { changeTemplate };
