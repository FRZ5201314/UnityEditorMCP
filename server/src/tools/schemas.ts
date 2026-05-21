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

const propertyValueSchema = z.union([
  z.string(),
  z.number(),
  z.boolean(),
  z.null(),
  z.object({ x: z.number(), y: z.number() }),
  z.object({ x: z.number(), y: z.number(), z: z.number() }),
  z.object({ r: z.number(), g: z.number(), b: z.number(), a: z.number().optional() }),
]);

export const componentPropertyGetSchema = {
  path: z.string().min(1),
  typeName: z.string().min(1),
  propertyPath: z.string().min(1),
};

export const componentPropertySetSchema = {
  path: z.string().min(1),
  typeName: z.string().min(1),
  propertyPath: z.string().min(1),
  value: propertyValueSchema.optional(),
  objectReferenceAssetPath: z.string().optional(),
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
  compileTimeoutMs: z.number().int().positive().optional(),
};

export const sceneNewSchema = {
  setup: z.enum(["DefaultGameObjects", "EmptyScene"]).optional(),
  mode: z.enum(["Single", "Additive"]).optional(),
};

export const sceneOpenSchema = {
  path: z.string().min(1),
  mode: z.enum(["Single", "Additive"]).optional(),
};

export const prefabCreateSchema = {
  path: z.string().min(1),
  assetPath: z.string().min(1),
};

export const prefabInstantiateSchema = {
  assetPath: z.string().min(1),
  parentPath: z.string().optional(),
};

export const assetFindSchema = {
  filter: z.string().min(1),
  folders: z.array(z.string().min(1)).optional(),
};

export const assetPathSchema = {
  assetPath: z.string().min(1),
};

export const assetCreateFolderSchema = {
  parentPath: z.string().optional(),
  name: z.string().min(1),
};

export const bridgeSelectSchema = {
  url: z.string().min(1).optional(),
  projectPath: z.string().min(1).optional(),
  projectName: z.string().min(1).optional(),
  instanceId: z.string().min(1).optional(),
};
