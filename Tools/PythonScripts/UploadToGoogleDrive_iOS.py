#-*- coding:utf-8 -*-
# pydrive 라이브러리 안쓰는 버전. 휴이님의 버그있다는 제보가 들어왔음.

from __future__ import print_function

import os.path
import time
import glob
import mimetypes
import requests
import json
import build

import google.auth
import google.auth.transport.requests

from google.oauth2.credentials import Credentials
from google_auth_oauthlib.flow import InstalledAppFlow
from googleapiclient.discovery import build
from googleapiclient.errors import HttpError
from googleapiclient.http import MediaFileUpload

# 이걸 수정하게 되면 Credentials 지우고 다시 생성해야 함.
SCOPES = ['https://www.googleapis.com/auth/drive']

# 빌드파일이 저장되는 경로.
BuildFiles_Path = '../oz_client/Build/iOS/build/Release-iphoneos'
# 빌드파일 확장자. 테스트.
TEST_Extention = r"*.plist"
# 빌드파일 확장자.
IPA_Extention = r"*.ipa"

# 빌드 파일 올라갈 구글 드라이브 폴더 아이디.
Upload_Folder_ID = '1Mjf5p94DXOl_iqnO8FakVwGu9OyDLUNX'

# 업로드 파일 링크할 슬랙 채널.
Alarm_Slack_Channel = '#오즈_빌드'


def main():
    # Google Drive 접근을 위한 Credentials
    credentials = check_credentials()

    # Google Drive 인스턴스 생성. v3 API.
    print('Connecting Google Drive')
    google_drive = build('drive', 'v3', credentials=credentials)

    # Read Google Drive Files.
    # read_drive_files(credentials, google_drive)

    # 가장 최근 빌드파일 추려내기.
    upload_file = get_newest_file()

    # 업로드.
    file_link = upload_file_to_google_drive(google_drive, upload_file)

    # slack_message(Alarm_Slack_Channel, file_link)


# Credential 체크.
def check_credentials():
    print('Check Credentials')
    credentials = None
    # 생성된 Credentials이 있는 경우.
    if os.path.exists('Credentials.json'):
        credentials = Credentials.from_authorized_user_file('Credentials.json', SCOPES)
    # 생성된 Credentials이 없거나 유효하지 않은 경우.
    if not credentials or not credentials.valid:
        # Credentials 만료. 갱신.
        if credentials and credentials.expired and credentials.refresh_token:
            credentials.refresh(google.auth.transport.requests.Request())
        # Credentials 새로 발급. 스크립트가 위치한 폴더의 client_secret.json 사용.
        else:
            flow = InstalledAppFlow.from_client_secrets_file('client_secret.json', SCOPES)
            credentials = flow.run_local_server(port=0)
        # 발급된 Credentials 로컬에 저장.
        with open('Credentials.json', 'w') as token:
            token.write(credentials.to_json())

    return credentials


# Google Drive 파일목록 읽기.
def read_drive_files(google_drive):
    try:
        # google_drive = build('drive', 'v3', credentials=credentials)

        # 파일 목록 읽기.
        results = google_drive.files().list(
            pageSize=10, fields="nextPageToken, files(id, name)").execute()

        items = results.get('files', [])

        if not items:
            print('No files found.')
            return

    except HttpError as error:
        # Google Drive 접근 에러.
        print(error.error_details)


# 파일 사이즈 체크.
def get_file_size(file_name):
    from os import stat
    file_stats = stat(file_name)
    print('File Size in Bytes is {}'.format(file_stats.st_size))
    return file_stats.st_size


# 파일 생성 날짜 체크.
def get_file_created_time(file_name):
    return os.path.getmtime(file_name)


# 지정폴더의 특정확장자명의 파일들을 검색한뒤 가장 늦게 생성된 파일을 반환.
def get_newest_file():
    print('Start Get Newest File')

    file_name = None
    create_time = 0.0
    files_count = 0

    # for file in os.listdir(os.path.abspath(os.path.join(os.pardir, BuildFiles_Path))):
    #     if file.endswith(APK_Extention):
    #         if create_time < os.path.getctime(file):
    #             create_time = os.path.getctime(file)
    #             file_name = file

    # 검색할 파일 경로와 확장자.
    search_target = os.path.abspath(os.path.join(os.pardir, BuildFiles_Path)) + '/' + IPA_Extention
    print('Searching Target Path : ' + search_target)

    for file in glob.glob(search_target):
        # print(file)
        # print(os.path.getctime(file))
        if create_time < os.path.getctime(file):
            create_time = os.path.getctime(file)
            file_name = file
            files_count += 1

    print('Searching Count : ' + str(files_count))
    print('Upload File Name : ' + file_name)

    return file_name


# Upload File to Google Drive.
def upload_file_to_google_drive(google_drive, upload_file):
    print('Start Upload')

    # mimetype 만들기. 정말 필요한 것인가?
    (mimetype, encoding) = mimetypes.guess_type(upload_file)
    if mimetype is None:
        mimetype = "application/octet-stream"

    # MediaFileUpload(업로드 파일, mimetype(업로드 파일타입), resumable(이어서 업로드 여부), chunksize(잘라서 업로드 할 크기))
    media_body = MediaFileUpload(upload_file, mimetype=mimetype, resumable=True)

    # 파일명으로 업로드.
    (directory, file) = os.path.split(upload_file)

    # 파일 이름정하고, 업로드할 폴더 id 지정.
    body = {
        'name': file,
        'parents': [Upload_Folder_ID]
    }

    # google_drive.files().create(body=body, media_body=media_body, fields='id').execute()
    result = google_drive.files().create(body=body, media_body=media_body, fields='mimeType,exportLinks,id').execute()

    print('Finish Upload')

    return 'Uploaded IPA to ' + format('https://drive.google.com/open?id=' + result.get('id'))


# Upload File to Google Drive(chunk).
def upload_file_to_google_drive_chunk(google_drive, upload_file, chunksize=262144):
    print('Start Upload(chunk)')

    # mimetype 만들기. 정말 필요한 것인가?
    (mimetype, encoding) = mimetypes.guess_type(upload_file)
    if mimetype is None:
        mimetype = "application/octet-stream"

    # MediaFileUpload(업로드 파일, mimetype(업로드 파일타입), resumable(이어서 업로드 여부), chunksize(잘라서 업로드 할 크기))
    media_body = MediaFileUpload(upload_file, mimetype=mimetype, resumable=True, chunksize=chunksize)

    # 파일명으로 업로드.
    (directory, file) = os.path.split(upload_file)

    # 파일 이름정하고, 업로드할 폴더 id 지정.
    body = {
        'title': file,
        'parents': [{'id': Upload_Folder_ID}]
    }

    request = google_drive.files().create(body=body, media_body=media_body).execute()
    if get_file_size(upload_file) > chunksize:
        response = None
        while response is None:
            chunk = request.next_chunk()
            if chunk:
                status, response = chunk
                if status:
                    print("Uploaded %d%%." % int(status.progress() * 100))

    print('Finish Upload')


# def upload_file_to_google_drive_query(access_token, upload_file, parent_id):
#     query_link = 'https://www.googleapis.com/upload/drive/v3/files'
#     query_params = "?uploadType=multipart"
#     link = '{0}{1}'.format(query_link, query_params)
#     headers = {"Authorization": "Bearer " + access_token}
#     meta_data = {
#         "title": upload_file,
#         "parents": [{"id": parent_id}]
#     }
#     files = {
#         "data": ("metadata", json.dumps(meta_data), "application/json; charset=UTF-8"),
#         "file": open(upload_file, 'rb')
#     }
#     response = requests.post(link, headers=headers, files=files)


# Slack에 메세지 보내기. Http 전송으로 간단하게.
def slack_message(channel, message):
    # macovill Bot app 토큰.
    slack_bot_token = "xoxb-957042924785-4288315328401-FUUjKBpWLOhv0aWRYwfJF1D1"
    headers = {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + slack_bot_token
    }
    payload = {
        'channel': channel,
        'text': message
    }
    requests.post('https://slack.com/api/chat.postMessage',
                  headers=headers,
                  data=json.dumps(payload)
                  )


if __name__ == '__main__':
    main()

########################################################################################################################
# # pydrive 라이브러리와 Google Service API 인증을 로컬에 저장하는 방식.
#
# # Google Service API 인증을 위한 라이브러리.
# from pydrive.auth import GoogleAuth
# # Google Drive 접근을 위한 라이브러리.
# from pydrive.drive import GoogleDrive
#
# # 스크립트가 위치한 폴더의 client_secret.json 파일을 이용해 Google Service API 인증.
# googleServiceAPIAuthentication = GoogleAuth()
#
# # 로컬의 Credentials 로딩.
# googleServiceAPIAuthentication.LoadCredentialsFile("Credentials.txt")
# # 로컬에 Credentials 이 없다면 생성.
# if googleServiceAPIAuthentication.credentials is None:
#     googleServiceAPIAuthentication.GetFlow()
#     googleServiceAPIAuthentication.flow.params.update({'access_type': 'offline'})
#     googleServiceAPIAuthentication.flow.params.update({'approval_prompt': 'force'})
#     googleServiceAPIAuthentication.LocalWebserverAuth()
# # Credentials 이 만료되었다면 갱신.
# elif googleServiceAPIAuthentication.access_token_expired:
#     googleServiceAPIAuthentication.Refresh()
# else:
#     googleServiceAPIAuthentication.Authorize()
#
# # Credentials 파일로 저장.
# googleServiceAPIAuthentication.SaveCredentialsFile("Credentials.txt")
#
# # Google Service API 인증으로 구글 드라이브 접근.
# googleDrive = GoogleDrive(googleServiceAPIAuthentication)
#
# # 빌드 파일 올라갈 구글 드라이브 폴더 아이디.
# UPLOAD_BUILD_PATH = '1vQenwWboAieDO_jDrB1nKVXEMaLB1FJB'
#
# # 스크립트가 위치한 폴더의 파일 올리기.
# # 파일 올라갈 구글 드라이브 폴더 아이디.
# UPLOAD_BUILD_PATH_TEST = '1vQenwWboAieDO_jDrB1nKVXEMaLB1FJB'
# # 업로드할 파일 리스트.
# listUploadFile = ['1.jpg', '2.jpg']
# # UPLOAD_BUILD_PATH_TEST 구글 드라이브 폴더에 업로드.
# for uploadFile in listUploadFile:
#     # 업로드할 구글 드라이브 폴더 아이디.
#     googleDriveFolder = googleDrive.CreateFile({'parents': [{'id': UPLOAD_BUILD_PATH_TEST}]})
#     # 업로드할 파일 세팅.
#     googleDriveFolder.SetContentFile(uploadFile)
#     # 업로드.
#     googleDriveFolder.Upload()
# # UPLOAD_BUILD_PATH_TEST 구글 드라이브 폴더의 모든 파일 목록 얻어오기.
# listGoogleDriveFile = googleDrive.ListFile(
#     {'q': "'{}' in parents and trashed=false".format(UPLOAD_BUILD_PATH_TEST)}).GetList()
# for file in listGoogleDriveFile:
#     print('title: %s, id: %s' % (file['title'], file['id']))
# # UPLOAD_BUILD_PATH_TEST 구글 드라이브 폴더의 모든 파일 다운로드.
# # 먼저 파일 목록을 얻어온 뒤에야 다운로드가 가능하다.
# for i, file in enumerate(sorted(listGoogleDriveFile, key=lambda x: x['title']), start=1):
#     print('Downloading {} file from GDrive ({}/{})'.format(file['title'], i, len(listGoogleDriveFile)))
#     file.GetContentFile(file['title'])
#
# # UPLOAD_BUILD_PATH_TEST 구글 드라이브 폴더에 파일 만들기.
# googleDriveFolder1 = googleDrive.CreateFile({'parents': [{'id': UPLOAD_BUILD_PATH_TEST}], 'title': 'test.txt'})
# # 만든 파일의 내용 입력.
# googleDriveFolder1.SetContentString('Hello World!')
# # 업로드.
# googleDriveFolder1.Upload()
#
# # UPLOAD_BUILD_PATH_TEST 구글 드라이브 폴더의 파일 내용 출력.
# googleDriveFolder1 = googleDrive.CreateFile({'id': googleDriveFolder1['id']})
# googleDriveFolder1.GetContentString('test.txt')
########################################################################################################################

########################################################################################################################
# # pydrive 라이브러리와 Settings.yaml 파일을 이용해 인증.
#
# # Google Service API 인증을 위한 라이브러리.
# from pydrive.auth import GoogleAuth
# # Google Drive 접근을 위한 라이브러리.
# from pydrive.drive import GoogleDrive
#
# # 스크립트가 위치한 폴더의 client_secret.json 파일을 이용해 Google Service API 인증.
# googleServiceAPIAuthentication = GoogleAuth()
# # Create local webserver and auto handles authentication.
# googleServiceAPIAuthentication.LocalWebserverAuth()
# # Google Service API 인증으로 구글 드라이브 접근.
# googleDrive = GoogleDrive(googleServiceAPIAuthentication)
#
# # 빌드 파일 올라갈 구글 드라이브 폴더 아이디.
# UPLOAD_BUILD_PATH = ''
# ########################################################################################################################
# # Test.
# # 스크립트가 위치한 폴더의 파일 올리기.
# # 파일 올라갈 구글 드라이브 폴더 아이디.
# UPLOAD_BUILD_PATH_TEST = '1vQenwWboAieDO_jDrB1nKVXEMaLB1FJB'
# # 업로드할 파일 리스트.
# listUploadFile = ['1.jpg', '2.jpg']
# # UPLOAD_BUILD_PATH_TEST 구글 드라이브 폴더에 업로드.
# for uploadFile in listUploadFile:
# 	# 업로드할 구글 드라이브 폴더 아이디.
# 	googleDriveFolder = googleDrive.CreateFile({'parents': [{'id': UPLOAD_BUILD_PATH_TEST}]})
# 	# 업로드할 파일 세팅.
# 	googleDriveFolder.SetContentFile(uploadFile)
# 	# 업로드.
# 	googleDriveFolder.Upload()
# # UPLOAD_BUILD_PATH_TEST 구글 드라이브 폴더의 모든 파일 목록 얻어오기.
# listGoogleDriveFile = googleDrive.ListFile(
# 	{'q': "'{}' in parents and trashed=false".format(UPLOAD_BUILD_PATH_TEST)}).GetList()
# for file in listGoogleDriveFile:
# 	print('title: %s, id: %s' % (file['title'], file['id']))
# # UPLOAD_BUILD_PATH_TEST 구글 드라이브 폴더의 모든 파일 다운로드.
# # 먼저 파일 목록을 얻어온 뒤에야 다운로드가 가능하다.
# for i, file in enumerate(sorted(listGoogleDriveFile, key=lambda x: x['title']), start=1):
# 	print('Downloading {} file from GDrive ({}/{})'.format(file['title'], i, len(listGoogleDriveFile)))
# 	file.GetContentFile(file['title'])
#
# # UPLOAD_BUILD_PATH_TEST 구글 드라이브 폴더에 파일 만들기.
# googleDriveFolder1 = googleDrive.CreateFile({'parents': [{'id': UPLOAD_BUILD_PATH_TEST}], 'title': 'test.txt'})
# # 만든 파일의 내용 입력.
# googleDriveFolder1.SetContentString('Hello World!')
# # 업로드.
# googleDriveFolder1.Upload()
#
# # UPLOAD_BUILD_PATH_TEST 구글 드라이브 폴더의 파일 내용 출력.
# googleDriveFolder1 = googleDrive.CreateFile({'id': googleDriveFolder1['id']})
# googleDriveFolder1.GetContentString('test.txt')
########################################################################################################################
