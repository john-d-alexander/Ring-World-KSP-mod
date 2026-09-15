"""Local Blender MCP client. All diagnostics stay under the mod directory."""
import argparse
import json
import pathlib
import socket

ROOT = pathlib.Path(__file__).resolve().parents[1]


def call(command, params=None, timeout=300):
    with socket.create_connection(('127.0.0.1', 9876), 10) as connection:
        connection.settimeout(timeout)
        connection.sendall(json.dumps({'type': command, 'params': params or {}}).encode())
        data = b''
        while True:
            chunk = connection.recv(65536)
            if not chunk:
                raise RuntimeError('Blender closed the connection before sending a complete result')
            data += chunk
            try:
                result = json.loads(data)
                break
            except json.JSONDecodeError:
                continue
    if result.get('status') != 'success':
        raise RuntimeError(json.dumps(result))
    return result


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--script', type=pathlib.Path)
    parser.add_argument('--timeout', type=int, default=300)
    parser.add_argument('--output', type=pathlib.Path)
    args = parser.parse_args()
    result = call('execute_code', {'code': args.script.read_text(encoding='utf-8')}, args.timeout) if args.script else call('get_scene_info')
    text = json.dumps(result, indent=2)
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(text, encoding='utf-8')
    print(text)
