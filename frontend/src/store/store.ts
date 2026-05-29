import { create } from 'zustand';

type ModalType = 'addClient' | 'addWarehouse'|'addProvider'|'addOrder'|'clientFilter'|'providerFilter'|'orderFilter'|'addEmployee'| null;

interface ModalState {
    activeModal: ModalType;
    openModal: (modal: ModalType) => void;
    closeModal: () => void;
}

export const useModalStore = create<ModalState>((set) => ({
    activeModal: null,
    openModal: (modal) => set({ activeModal: modal }),
    closeModal: () => set({ activeModal: null }),
}));
