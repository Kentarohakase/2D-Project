using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

// Verwenden von "Touch" aus EnhancedTouch, um Namenskonflikte mit anderen Touch-Klassen zu vermeiden.
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class BallHandler : MonoBehaviour {
 [SerializeField]
 private GameObject ballTemplate; // Vorlage für den Ball, die zur Erstellung neuer Ballinstanzen im Spiel verwendet wird.
 [SerializeField]
 private Rigidbody2D pivotRigidbody; // Der Rigidbody des Pivot-Objekts, an dem der Ball mit einem SpringJoint befestigt wird.
 [SerializeField]
 private float launchDuration; // Zeit in Sekunden, die bestimmt, wie lange der Ball nach dem Loslassen gezogen wird, bevor er abgeschossen wird.
 [SerializeField]
 private float respawnTime; // Zeit in Sekunden, bis ein neuer Ball nach dem Abschuss des aktuellen Balls erscheint.

 private Camera mainCamera; // Referenz auf die Hauptkamera des Spiels, wird verwendet, um Bildschirmkoordinaten in Weltkoordinaten zu übersetzen.
 private bool isBeingDragged; // Ein boolscher Wert, der angibt, ob der Ball gerade vom Spieler gezogen wird.
 private Rigidbody2D activeBallRigidbody; // Rigidbody der aktuell aktiven Ballinstanz.
 private SpringJoint2D activeBallSpringJoint; // SpringJoint-Komponente der aktuell aktiven Ballinstanz.
 private float ballDestructionDelay = 2; // Zeit in Sekunden, nach der der Ball automatisch zerstört wird, nachdem er vom Pivot getrennt wurde.

 void Start( )
 {
  mainCamera = Camera.main; // Initialisiere die Hauptkamera.
  SpawnBall( ); // Erzeuge beim Start des Spiels die erste Ballinstanz.
 }

 private void OnEnable( )
 {
  EnhancedTouchSupport.Enable( ); // Aktiviere Enhanced Touch Support bei Aktivierung des Skripts.
 }

 private void OnDisable( )
 {
  EnhancedTouchSupport.Disable( ); // Deaktiviere Enhanced Touch Support, wenn das Skript deaktiviert wird.
 }

 void Update( )
 {
  // Überprüfe, ob der aktuelle Ball und sein SpringJoint existieren, bevor weitere Aktionen ausgeführt werden.
  if (activeBallRigidbody == null && activeBallSpringJoint == null)
  {
   return;
  }

  // Behandle den Zustand, wenn keine Berührungen aktiv sind.
  if (Touch.activeTouches.Count == 0)
  {
   if (isBeingDragged)
   {
    LaunchBall( ); // Starte den Ball, wenn der Benutzer aufgehört hat zu ziehen.
   }
   isBeingDragged = false;
   return;
  }

  // Wenn es aktive Berührungen gibt, markiere den Ball als gezogen und setze seinen Rigidbody auf kinematisch.
  isBeingDragged = true;
  activeBallRigidbody.isKinematic = true;

  // Berechne die durchschnittliche Position aller aktiven Berührungen.
  Vector2 touchPosition = new Vector2( );
  foreach (Touch touch in Touch.activeTouches)
  {
   touchPosition += touch.screenPosition;
  }
  touchPosition /= Touch.activeTouches.Count;

  // Konvertiere die durchschnittliche Bildschirmposition der Berührungen in eine Weltkoordinate.
  Vector3 worldPosition = mainCamera.ScreenToWorldPoint( touchPosition );
  activeBallRigidbody.position = new Vector2( worldPosition.x , worldPosition.y );
 }

 private void LaunchBall( )
 {
  // Deaktiviere den kinematischen Zustand des Rigidbody, um Physikeffekte zu ermöglichen, und trenne die Verbindung zum Ball.
  activeBallRigidbody.isKinematic = false;
  activeBallRigidbody = null; // Entferne die Referenz auf den Rigidbody, um zu verhindern, dass er in der nächsten Frame weiter manipuliert wird.

  Invoke( nameof( DetachBall ) , launchDuration ); // Plane das Trennen des Balls vom SpringJoint nach der festgelegten Verzögerung.
 }

 private void DetachBall( )
 {
  // Stelle sicher, dass der SpringJoint existiert, bevor du versuchst, ihn zu deaktivieren.
  if (activeBallSpringJoint != null)
  {
   activeBallSpringJoint.enabled = false; // Deaktiviere den SpringJoint, um den Ball loszulassen.
   StartCoroutine( DestroyBallAfterDelay( activeBallSpringJoint.gameObject , ballDestructionDelay ) ); // Beginne den Zerstörungs-Countdown für den Ball.
   activeBallSpringJoint = null; // Entferne die Referenz auf den SpringJoint, um weitere Manipulationen zu verhindern.
  }
  else
  {
   Debug.LogError( "activeBallSpringJoint is null in DetachBall" ); // Gib einen Fehler aus, wenn der SpringJoint zum Zeitpunkt des Trennens nicht existiert.
  }

  Invoke( nameof( SpawnBall ) , respawnTime ); // Plane das Erscheinen eines neuen Balls nach der festgelegten Verzögerung.
 }

 private IEnumerator DestroyBallAfterDelay( GameObject ball , float delay )
 {
  yield return new WaitForSeconds( delay ); // Warte die angegebene Zeit, bevor der Ball zerstört wird.

  if (ball != null) // Überprüfe, ob der Ball noch existiert, bevor du versuchst, ihn zu zerstören.
  {
   Destroy( ball ); // Zerstöre den Ball.
  }
 }

 private void SpawnBall( )
 {
  // Erzeuge eine neue Ballinstanz an der Position des Drehpunkts und initialisiere seine Rigidbody- und SpringJoint-Komponenten.
  GameObject ballInstance = Instantiate( ballTemplate , pivotRigidbody.position , Quaternion.identity );
  activeBallRigidbody = ballInstance.GetComponent<Rigidbody2D>( );
  activeBallSpringJoint = ballInstance.GetComponent<SpringJoint2D>( );
  activeBallSpringJoint.connectedBody = pivotRigidbody; // Verbinde den neuen Ball mit dem Drehpunkt.
 }
}
