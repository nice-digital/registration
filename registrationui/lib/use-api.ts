import React, { useEffect, useState } from 'react';

function initialState(args : any) {
  return {
    response: null,
    error: null,
    isLoading: true,
    ...args
  };
}

const useApi = (url : string, options: any = {}) => {
  const [state, setState] = useState(() => initialState({}));

  useEffect(() => {
    const fetchData = async () => {
      try {

        console.log("fetching");
        const res = await fetch(url, {
          ...options
        });

        console.log("res.status:" + res.status);
        if (res.status >= 400) {
          setState(
            initialState({
              error: await res.json(),
              isLoading: false
            })
          );
        } else {
          setState(
            initialState({
              response: await res.json(),
              isLoading: false
            })
          );
        }
      } catch (error) {
        setState(
          initialState({
            error: {
              error: error.message
            },
            isLoading: false
          })
        );
      }
    };
    fetchData();
  }, []);
  return state;
};

export default useApi;
