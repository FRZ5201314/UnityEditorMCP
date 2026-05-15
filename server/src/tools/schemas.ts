import { z } from "zod";

export const emptySchema = {};

export const pathSchema = {
  path: z.string().min(1),
};

export const vector3Schema = z.object({
  x: z.number(),
  y: z.number(),
  z: z.number(),
});

export const hierarchyListSchema = {
  rootPath: z.string().optional(),
  recursive: z.boolean().optional(),
};

export const gameObjectCreateSchema = {
  name: z.string().optional(),
  parentPath: z.string().optional(),
};

export const gameObjectRenameSchema = {
  path: z.string().min(1),
  name: z.string().min(1),
};

export const transformSetSchema = {
  path: z.string().min(1),
  position: vector3Schema.optional(),
  localPosition: vector3Schema.optional(),
  rotation: vector3Schema.optional(),
  localRotation: vector3Schema.optional(),
  scale: vector3Schema.optional(),
};

export const componentSchema = {
  path: z.string().min(1),
  typeName: z.string().min(1),
};

export const scriptCreateSchema = {
  assetPath: z.string().min(1),
  className: z.string().optional(),
  content: z.string().optional(),
  overwrite: z.boolean().optional(),
};

export const scriptAttachSchema = {
  path: z.string().min(1),
  typeName: z.string().min(1),
};
