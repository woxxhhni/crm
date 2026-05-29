import '@tanstack/react-query';

declare module '@tanstack/react-query' {
    interface Register {
        defaultError: Error;
    }

    interface QueryMeta {
        showErrorNotification?: boolean;
    }

    interface MutationMeta {
        showErrorNotification?: boolean;
    }
}
