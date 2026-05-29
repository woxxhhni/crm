export const ClientEndpoints = {
    root: '/api/v1/Clients',
    byId:(id:string|number)=> `/api/v1/Clients/${id}`,
    fileGroups:(id:string|number)=>`/api/v1/Clients/${id}/file-groups`,
    filesGroupAction:(clientId:string|number,fileId:string|number)=>`/api/v1/Clients/${clientId}/file-groups/${fileId}`,
    filesGroupItemAction:(clientId:string|number,fileId:string|number)=>`/api/v1/Clients/${clientId}/file-groups/${fileId}/items`,
    filesGroupItemDelete:(clientId:string|number,fileId:string|number)=>`/api/v1/Clients/${clientId}/file-groups/items/${fileId}`
} as const;
