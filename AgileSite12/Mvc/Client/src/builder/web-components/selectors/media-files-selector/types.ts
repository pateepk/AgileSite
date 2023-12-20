import { ModalDialogApplyCallback, ModalDialogOptions } from "@/builder/declarations";

export interface MediaFilesSelectorItem {
  fileGuid: string;
}

export interface MediaFile {
  readonly name: string;
  readonly extension: string;
  readonly fileGuid: string;
  readonly url: string;
  readonly thumbnailUrls?: MediaFileThumbnails;
  readonly mimeType: string;
  readonly size: number;
  readonly title: string;
  readonly description: string;
  readonly folderPath: string;
  readonly libraryName: string;
  readonly siteName: string;
  readonly isValid: boolean;
}

export interface MediaFileThumbnails {
  readonly small: string;
  readonly medium: string;
  readonly large: string;
}

export interface MediaLibrary {
  readonly name: string;
  readonly identifier: string;
  readonly createFile: boolean;
}

export interface UploaderOptions {
  readonly allowedExtensions: string;
  readonly showEmptyContainer: boolean;
  readonly enabled: boolean;
  readonly title: string;
  readonly uploadHandler: () => void;
  readonly url: string;
}

export interface MediaFilesDialogOptions extends ModalDialogOptions {
  /**
   * Code name of the media library.
   */
  readonly libraryName?: string;

  /**
   * Limit of maximum number of files allowed to be selected.
   * 0 - unlimited
   * 1 - single file selection
   * n - n-files selection
   */
  readonly maxFilesLimit: number;

  /**
   * List of file extensions starting with a dot.
   */
  readonly allowedExtensions: string;

  /**
   * List of initially selected file items.
   */
  readonly selectedValues: MediaFilesSelectorItem[];

  /**
   * Function which is called when the apply button on the dialog is clicked.
   */
  readonly applyCallback: (files: MediaFile[]) => ModalDialogApplyCallback;
}
