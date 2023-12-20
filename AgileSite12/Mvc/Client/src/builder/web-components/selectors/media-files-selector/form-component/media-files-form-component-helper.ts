import { GetString } from "../../selector-types";
import { MediaFile } from "../types";

/**
 * Returns error text for the given invalid file.
 * @param {MediaFile} invalidFile. Invalid media file.
 * @param {GetString} getString. Function callback for strings localization.
 */
const getErrorText = (invalidFile: MediaFile, getString: GetString): string => {
  if (!invalidFile) {
    return "";
  }

  if (!invalidFile.name) {
    return getString("kentico.components.mediafileselector.missingfile");
  }
  return getString("kentico.components.mediafileselector.fileerror");
};

/**
 * Returns filename of the given file. When the given file is invalid, it returns appropriate error text.
 * @param {MediaFile} file. Media file.
 * @param {GetString} getString. Function callback for strings localization.
 */
const getFileName = (file: MediaFile, getString: GetString) => {
  if (!file) {
    return "";
  }

  if (!file.isValid) {
    return getFileNameWithErrorText(file, getString);
  }

  return getCompleteFilename(file);
};

/**
 * Returns title text for the given file. When the given file is invalid, it returns appropriate error text.
 * @param {MediaFile} file. Media file.
 * @param {GetString} getString. Function callback for strings localization.
 */
const getTitle = (file: MediaFile, getString: GetString): string => {
  if (!file) {
    return "";
  }

  if (file.isValid) {
    return getFileName(file, getString);
  }

  if (file.name) {
    return `${getCompleteFilename(file)} - ${getString("kentico.components.mediafileselector.fileerror.title")}`;
  } else {
    return getString("kentico.components.mediafileselector.missingfile.title");
  }
};

const getCompleteFilename = (file: MediaFile): string => file.name + file.extension;

const getFileNameWithErrorText = (file: MediaFile,  getString: GetString): string => {
  if (!file.name) {
    return getString("kentico.components.mediafileselector.missingfile");
  }
  return `${getErrorText(file, getString)} - ${getCompleteFilename(file)}`;
};

export {
  getFileName,
  getTitle,
  getErrorText,
};
