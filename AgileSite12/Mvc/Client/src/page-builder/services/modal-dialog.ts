import { http } from "@/builder/api/client";
import { PropertiesValidationResponseData } from "@/builder/declarations";
import { arrayHelper } from "@/builder/helpers";

/**
 * Sends properties to the server and receives properties form HTML markup.
 * @param propertiesFormUrl URL where the form HTML can be retrieved.
 * @param properties Properties for which the form should be generated.
 * @returns HTML markup of the properties form.
 */
const getPropertiesDialogMarkup = async (propertiesFormUrl: string, properties: object): Promise<string> => {
  const response = await http.post(propertiesFormUrl, JSON.stringify({ properties }));

  return response.data;
};

/**
 * Posts given formData to the given endpointUrl using HTTP POST method and returns response result.
 * @param endpointUrl URL of the endpoint where data should be sent.
 * @param formData Form data.
 * @returns Validation response result.
 */
const postForm = async (endpointUrl: string, formData: FormData): Promise<PropertiesValidationResponseData> => {
  const response = await http.post(endpointUrl, formData);

  if (arrayHelper.contains(response.headers["content-type"], "text/html")) {
    return Promise.resolve({
      data: response.data,
      status: "invalid",
    });
  }

  if (arrayHelper.contains(response.headers["content-type"], "application/json")) {
    return Promise.resolve({
      data: response.data,
      status: "valid",
    });
  }

  return Promise.resolve({
    data: null,
    status: "invalid",
  });
};

export {
  getPropertiesDialogMarkup,
  postForm,
};
