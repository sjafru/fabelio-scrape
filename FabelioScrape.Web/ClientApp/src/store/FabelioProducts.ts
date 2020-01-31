import { Action, Reducer } from 'redux';
import { AppThunkAction } from '.';

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface AddFabelioProductState {
    productUrl: string;
}

export interface ListFabelioProductsState {
    isLoading: boolean;
    startDateIndex?: number;
    products: FabelioProduct[];
}

export interface FabelioProduct {
    id: string;
    title: string;
    subTitle: string;
    imageUrls: Array<string>;
    description: string;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

export interface ReqAddFabelioProductAction { type: 'REQ_ADD_FABELIO_PRODUCT', productUrl: string }
export interface ResAddFabelioProductAction { type: 'RES_ADD_FABELIO_PRODUCT', productUrl: string, product: FabelioProduct }

interface ReqListFabelioProductsAction {
    type: 'REQ_LIST_FABELIO_PRODUCTS';
    startDateIndex: number;
}

interface ResListFabelioProductsAction {
    type: 'RES_LIST_FABELIO_PRODUCTS';
    startDateIndex: number;
    products: FabelioProduct[];
}


// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = ReqAddFabelioProductAction | ResAddFabelioProductAction | ReqListFabelioProductsAction | ResListFabelioProductsAction;


// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
    addFabelioProduct: (productUrl: string): AppThunkAction<KnownAction> => (dispatch, getState) => {
        // Only load data if it's something we don't already have (and are not already loading)
        const appState = getState();
        if (appState && appState.addFabelioProduct && productUrl !== appState.addFabelioProduct.productUrl) {
            fetch(`products`, {
                method: 'post',
                body: JSON.stringify({ "fabelioProductURL": productUrl })
            })
                .then(response => response.json() as Promise<FabelioProduct>)
                .then(data => {
                    dispatch({ type: 'RES_ADD_FABELIO_PRODUCT', productUrl: productUrl, product: data });
                });

            dispatch({ type: 'REQ_ADD_FABELIO_PRODUCT', productUrl: productUrl });
        }
    },
    
    requestListProducts: (startDateIndex: number): AppThunkAction<KnownAction> => (dispatch, getState) => {
        // Only load data if it's something we don't already have (and are not already loading)
        const appState = getState();
        if (appState && appState.weatherForecasts && startDateIndex !== appState.weatherForecasts.startDateIndex) {
            fetch(`products`)
                .then(response => response.json() as Promise<FabelioProduct[]>)
                .then(data => {
                    dispatch({ type: 'RES_LIST_FABELIO_PRODUCTS', startDateIndex: startDateIndex, products: data });
                });

            dispatch({ type: 'REQ_LIST_FABELIO_PRODUCTS', startDateIndex: startDateIndex });
        }
    }
};


// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

export const addFabelioProductReducer: Reducer<AddFabelioProductState> = (state: AddFabelioProductState | undefined, incomingAction: Action): AddFabelioProductState => {
    if (state === undefined) {
        return { productUrl: '' };
    }

    const action = incomingAction as KnownAction;
    switch (action.type) {
        case 'REQ_ADD_FABELIO_PRODUCT':
            return { productUrl: state.productUrl };
        case 'RES_ADD_FABELIO_PRODUCT':
            return { productUrl: state.productUrl };
        default:
            return state;
    }
};

const unloadedState: ListFabelioProductsState = { products: [], isLoading: false };

export const listFabelioProductReducer: Reducer<ListFabelioProductsState> = (state: ListFabelioProductsState | undefined, incomingAction: Action): ListFabelioProductsState => {
    if (state === undefined) {
        return unloadedState;
    }

    const action = incomingAction as KnownAction;
    switch (action.type) {
        case 'REQ_LIST_FABELIO_PRODUCTS':
            return {
                startDateIndex: action.startDateIndex,
                products: state.products,
                isLoading: true
            };
        case 'RES_LIST_FABELIO_PRODUCTS':
            // Only accept the incoming data if it matches the most recent request. This ensures we correctly
            // handle out-of-order responses.
            if (action.startDateIndex === state.startDateIndex) {
                return {
                    startDateIndex: action.startDateIndex,
                    products: action.products,
                    isLoading: false
                };
            }
            break;
    }

    return state;
};

