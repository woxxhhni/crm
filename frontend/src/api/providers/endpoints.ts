export const ProvidersEndpoints = {
    root: '/api/v1/Providers',
    byId:(id:string|number)=> `/api/v1/Providers/${id}`,
    fileGroups:(id:string|number)=>`/api/v1/Providers/${id}/file-groups`,
    filesGroupAction:(providerId:string|number,fileId:string|number)=>`/api/v1/Providers/${providerId}/file-groups/${fileId}`,
    filesGroupItemAction:(providerId:string|number,fileId:string|number)=>`/api/v1/Providers/${providerId}/file-groups/${fileId}/items`,
    filesGroupItemDelete:(providerId:string|number,fileId:string|number)=>`/api/v1/Providers/${providerId}/file-groups/items/${fileId}`
} as const;
