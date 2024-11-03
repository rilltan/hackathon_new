# for face tracking we followed tutorial at https://medium.com/@aiphile/detecting-face-at-30-fps-on-cpu-on-mediapipe-python-dda264e26f20

import time
import cv2 as cv
import mediapipe as mp
import numpy as np

import socket

host, port = "127.0.0.1", 25001

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

try:
    sock.connect((host, port))
except:
    print("connection failed")
    exit()

mp_face_detection = mp.solutions.face_detection
cap = cv.VideoCapture(0)

with mp_face_detection.FaceDetection(model_selection=1, min_detection_confidence=0.5) as face_detector:
    frame_counter = 0
    fonts = cv.FONT_HERSHEY_PLAIN
    start_time = time.time()

    last_right_eye = (0,0)
    while True:
        frame_counter += 1
        ret, frame = cap.read()
        print(frame.shape)
        if ret is False:
            break
        rgb_frame = cv.cvtColor(frame, cv.COLOR_BGR2RGB)

        results = face_detector.process(rgb_frame)
        frame_height, frame_width, c = frame.shape
        if results.detections:
            right_eyes = []
            left_eyes = []
            bounding_box_heights = []
            for face in results.detections:
                face_react = np.multiply(
                    [
                        face.location_data.relative_bounding_box.xmin,
                        face.location_data.relative_bounding_box.ymin,
                        face.location_data.relative_bounding_box.width,
                        face.location_data.relative_bounding_box.height,
                    ],
                    [frame_width, frame_height, frame_width, frame_height]).astype(int)
                
                cv.rectangle(frame, face_react, color=(255, 255, 255), thickness=2)
                key_points = np.array([(p.x, p.y) for p in face.location_data.relative_keypoints])
                key_points_coords = np.multiply(key_points,[frame_width,frame_height]).astype(int)
                cv.circle(frame, key_points_coords[0], 4, (255, 255, 255), 2)
                cv.circle(frame, key_points_coords[0], 2, (0, 0, 0), -1)
                cv.circle(frame, key_points_coords[1], 4, (255, 255, 255), 2)
                cv.circle(frame, key_points_coords[1], 2, (0, 0, 0), -1)
                    
                
                right_eyes.append((key_points[0][0],key_points[0][1]))
                left_eyes.append((key_points[1][0],key_points[1][1]))
                bounding_box_heights.append(face.location_data.relative_bounding_box.height)
                
            min_distance = 99999
            current_eye_index = 0
            for i,eye in enumerate(right_eyes):
                distance = (eye[0]-last_right_eye[0])**2 + (eye[1]-last_right_eye[1])**2
                if distance < min_distance:
                    min_distance = distance
                    current_eye_index = i
            last_right_eye = right_eyes[current_eye_index]

            DATA = f"{right_eyes[current_eye_index][0]},{right_eyes[current_eye_index][1]},{left_eyes[current_eye_index][0]},{left_eyes[current_eye_index][1]},{bounding_box_heights[current_eye_index]}"

            sock.sendall(DATA.encode("utf-8"))

        fps = frame_counter / (time.time() - start_time)
        cv.putText(frame,f"FPS: {fps:.2f}",(30, 30),cv.FONT_HERSHEY_DUPLEX,0.7,(0, 255, 255),2,)
        cv.imshow("frame", frame)
        key = cv.waitKey(1)
        if key == ord("q"):
            break
    cap.release()
    cv.destroyAllWindows() 
    
    sock.close()