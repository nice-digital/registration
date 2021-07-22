import Head from 'next/head'
import styles from '../styles/Home.module.css'

import { useState, useEffect } from 'react';
import { registrationService } from '../services/registration.service';

export default function Register() {

  const [registration, setRegistration] = useState(null);

  const defaultRegistrationJSON = '{"registration":{ "userNameIdentifier":"someuserid", "firstName":"John" }';

  useEffect(() => {
    registrationService.getRegistrations().then((x :any) => setRegistration(x));
    }, []);

    // function deleteUser(id) {
    //     setUsers(users.map(x => {
    //         if (x.id === id) { x.isDeleting = true; }
    //         return x;
    //     }));
    //     userService.delete(id).then(() => {
    //         setUsers(users => users.filter(x => x.id !== id));
    //     });
    // }


  //if (isLoading) return <div>Loading...</div>;
  //if (error) return <div>{error.message}</div>;
  
   //}

  return (
    <div className={styles.container}>
      <Head>
        <title>Stakeholder reg form</title>
      </Head>

      <main className={styles.main}>

        JSON to save:
        <textarea value={""} />
        <br/><br/>
     
        {/* <button onClick={() => setRegistration()} className="btn btn-sm btn-danger btn-delete-user" disabled={user.isDeleting}>                                   
                                         <span>Save</span>                                    
                                </button> */}

      </main>

    </div>
  )
}

export {}